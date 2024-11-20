#!/usr/bin/env python3
import urllib.request, logging, subprocess, os, shutil, re, yaml, tempfile

# Inspired by https://github.com/radixdlt/radixdlt-python-clients
# Requires python 3+ and `pip install pyyaml`

logger = logging.getLogger()
logging.basicConfig(format='%(asctime)s [%(levelname)s]: %(message)s', level=logging.INFO)

PACKAGE_NAME='RadixDlt.CoreApiSdk'
API_SCHEMA_LOCATION = f'../src/{PACKAGE_NAME}/core-api-spec-copy.yaml'
API_GENERATED_DESTINATION = f'../src/{PACKAGE_NAME}/generated'

OPENAPI_GENERATION_FOLDER='.'
OPENAPI_TEMP_GENERATION_FOLDER=tempfile.TemporaryDirectory().name
OPENAPI_GENERATOR_FIXED_VERSION_JAR=os.path.join(OPENAPI_GENERATION_FOLDER, 'openapi-generator-cli-6.1.1-custom.jar')

def safe_os_remove(path, silent = False):
    try:
        shutil.rmtree(path) if os.path.isdir(path) else os.remove(path)
    except Exception as e:
        if not silent: logger.warning(e)

def replace_in_file(filename, target, replacement):
    with open(filename, 'r') as file:
        file_contents = file.read()
    file_contents = file_contents.replace(target, replacement)
    with open(filename, 'w') as file:
        file.write(str(file_contents))

def find_in_file_multiline(filename, regex):
    with open(filename, 'r') as file:
        file_contents = file.read()
    return re.findall(regex, file_contents)

def create_file(filename, file_contents):
    with open(filename, 'w') as file:
        file.write(str(file_contents))

def copy_file(source, dest):
    shutil.copyfile(source, dest)

def run(command, cwd = '.', should_log = False):
    if (should_log): logging.debug('Running cmd: %s' % command)
    response = subprocess.run(' '.join(command), cwd=cwd, shell=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    stderr = response.stderr.decode('utf-8')
    if response.returncode != 0: raise Exception(stderr)
    stdout = response.stdout.decode('utf-8')
    if (should_log): logging.debug('Response: %s', stdout)
    return stdout

"""
Some context:
Non-required non-nullable C# value types such as int/long/boolean are incorrectly serialized with their default (zeroed)
value while (de)serializing. We need to overwrite their backing types with:
- nullable definition (think: int => int?)
- EmitDefaultValue configured to false not to serialize them as nulls

Known limitations:
- does not resolve $ref properties
"""
nonreq_nonnull_types = []

def nonreq_nonnull_pre_processing(schema):
    types = ["number", "integer", "boolean"]
    components = schema['components']['schemas']

    for schema, sv in components.items():
        props = sv.get('properties')
        required = sv.get('required') or []
        if props is None: continue
        if schema.endswith('OptIns') or schema.endswith('Options'): continue

        for property, pv in props.items():
            if property in required: continue
            if pv.get('nullable') == 'true': continue
            if not pv.get('type') in types: continue

            pv['nullable'] = 'true'
            nonreq_nonnull_types.append((schema,property))

def nonreq_nonnull_post_processing(tmp_base_path):
    for schema, property in nonreq_nonnull_types:
        logging.info(f'Overwritting {schema}.{property}\'s EmitDefaultValue to false')

        filename = os.path.join(tmp_base_path, 'Model', f'{schema}.cs')

        replace_in_file(filename, f'[DataMember(Name = "{property}", EmitDefaultValue = true)]', f'[DataMember(Name = "{property}", EmitDefaultValue = false)]')


def prepare_schema_for_generation(original_schema_file, api_schema_temp_filename):
    with open(original_schema_file, 'r') as file:
        schema = yaml.safe_load(file)

    # Open API generator only works with 3.0.0
    schema['openapi'] = '3.0.0'
    nonreq_nonnull_pre_processing(schema)

    with open(api_schema_temp_filename, 'w') as file:
        yaml.dump(schema, file, sort_keys=False)

def generate_models(prepared_spec_file, tmp_client_folder, out_location):
    safe_os_remove(tmp_client_folder, True)
    run(['java', '-jar', OPENAPI_GENERATOR_FIXED_VERSION_JAR, 'generate',
         '-g', 'csharp-netcore',
         '-i', prepared_spec_file,
         '-o', tmp_client_folder,
         '--library', 'httpclient',
         # '-t', os.path.join(OPENAPI_GENERATION_FOLDER, 'template-overrides'),
         f'--additional-properties=packageName={PACKAGE_NAME},targetFramework=net6.0,optionalEmitDefaultValues=true,useOneOfDiscriminatorLookup=true,validatable=false'
    ], should_log=False)

    logging.info("Successfully generated.")

    copy_base_path = os.path.join(tmp_client_folder, 'src', PACKAGE_NAME)

    safe_os_remove(os.path.join(copy_base_path, f'{PACKAGE_NAME}.csproj'), silent=True)
    nonreq_nonnull_post_processing(copy_base_path)

    safe_os_remove(out_location, silent=True)
    shutil.copytree(copy_base_path, out_location)
    safe_os_remove(tmp_client_folder, silent=True)

    logging.info("Successfully fixed up.")

if __name__ == "__main__":
    logger.info('Will generate models from the API specifications')

    # Set working directory to be the same directory as this script
    os.chdir(os.path.dirname(os.path.abspath(__file__)))

    safe_os_remove(OPENAPI_TEMP_GENERATION_FOLDER, silent=True)
    os.makedirs(OPENAPI_TEMP_GENERATION_FOLDER)

    logging.info('Loading API Schema from {}, and preparing it...'.format(os.path.abspath(API_SCHEMA_LOCATION)))

    api_schema_temp_filename = os.path.join(OPENAPI_TEMP_GENERATION_FOLDER, 'core-api-spec-copy.yaml')
    prepare_schema_for_generation(API_SCHEMA_LOCATION, api_schema_temp_filename)

    logging.info('Generating code from prepared schema...')

    generate_models(api_schema_temp_filename, os.path.join(OPENAPI_TEMP_GENERATION_FOLDER, "csharp"), API_GENERATED_DESTINATION)

    logging.info("Code has been created.")

    safe_os_remove(OPENAPI_TEMP_GENERATION_FOLDER, silent=True)

    logging.info("Temp directory cleared.")

    run(['sh', 'ensure-license-headers.sh'], should_log=False)

    logging.info("Licence headers added.")
