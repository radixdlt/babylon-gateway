import urllib.request, logging, subprocess, os, shutil, re

# Inspired by https://github.com/radixdlt/radixdlt-python-clients
# Requires python 3+ and various packages above

logger = logging.getLogger()
logging.basicConfig(format='%(asctime)s [%(levelname)s]: %(message)s', level=logging.INFO)

API_SCHEMA_LOCATION = '../../../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-schema.yaml'
API_GENERATED_DESTINATION = '../lib/generated'

OPENAPI_GENERATION_FOLDER='.'
OPENAPI_TEMP_GENERATION_FOLDER='./temp'
OPENAPI_GENERATOR_FIXED_VERSION_JAR=os.path.join(OPENAPI_GENERATION_FOLDER, 'openapi-generator-cli-6.1.0.jar')
OPENAPI_GENERATOR_FIXED_VERSION_DOWNLOAD_URL='https://search.maven.org/remotecontent?filepath=org/openapitools/openapi-generator-cli/6.1.0/openapi-generator-cli-6.1.0.jar'

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

def generate_models(spec_file, tmp_client_folder, out_location):
    safe_os_remove(tmp_client_folder, True)
    # See https://openapi-generator.tech/docs/generators/typescript-fetch/
    run(['java', '-jar', OPENAPI_GENERATOR_FIXED_VERSION_JAR, 'generate',
         '-g', 'typescript-fetch',
         '-i', spec_file,
         '-o', tmp_client_folder,
         '--additional-properties=supportsES6=true,modelPropertyNaming=original,npmVersion=0.1.0'
    ], should_log=False)

    logging.info("Successfully generated.")

    def fix_runtime(file_path):
        # For some reason it outputs invalid types for response, this seems to fix it
        replace_in_file(file_path, "let response = undefined;", "let response: Response = undefined as any as Response;")

    fix_runtime(os.path.join(tmp_client_folder, 'runtime.ts'))

    safe_os_remove(out_location, silent=True)
    shutil.copytree(tmp_client_folder, out_location)
    safe_os_remove(tmp_client_folder, silent=True)

    logging.info("Successfully fixed up.")

if __name__ == "__main__":
    logger.info('Will generate models from the API specifications')

    # Set working directory to be the same directory as this script
    os.chdir(os.path.dirname(os.path.abspath(__file__)))

    # check & download the openapi-generator.jar
    if not os.path.exists(OPENAPI_GENERATOR_FIXED_VERSION_JAR):
        logger.info('%s does not exist' % OPENAPI_GENERATOR_FIXED_VERSION_JAR)
        logger.info('Will download it from %s...' % OPENAPI_GENERATOR_FIXED_VERSION_DOWNLOAD_URL)
        urllib.request.urlretrieve(OPENAPI_GENERATOR_FIXED_VERSION_DOWNLOAD_URL, OPENAPI_GENERATOR_FIXED_VERSION_JAR)
        logger.info('Testing the openapi-generator...')
        logger.info(run(['ls %s' % OPENAPI_GENERATION_FOLDER]))
        run(['java', '-jar', OPENAPI_GENERATOR_FIXED_VERSION_JAR, 'author'], should_log=False)
        logger.info('All good.')

    safe_os_remove(OPENAPI_TEMP_GENERATION_FOLDER, silent=True)
    os.makedirs(OPENAPI_TEMP_GENERATION_FOLDER)

    api_schema_temp_filename = os.path.join(OPENAPI_TEMP_GENERATION_FOLDER, 'gateway-api-schema.yaml')
    copy_file(API_SCHEMA_LOCATION, api_schema_temp_filename)
    replace_in_file(api_schema_temp_filename, 'openapi: 3.1.0', 'openapi: 3.0.0')
    logging.info('Loaded API Schema from {}'.format(os.path.abspath(API_SCHEMA_LOCATION)))

    logging.info('Generating code from schema...')

    generate_models(api_schema_temp_filename, os.path.join(OPENAPI_TEMP_GENERATION_FOLDER, "typescript"), API_GENERATED_DESTINATION)

    logging.info("Code has been created.")

    safe_os_remove(OPENAPI_TEMP_GENERATION_FOLDER, silent=True)

    logging.info("Temp directory cleared.")
