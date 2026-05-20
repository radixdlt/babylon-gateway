Whenever new changes are released in the Gateway API, it is important to update the public documentation specs.

1. run `npx @redocly/cli@latest build-docs gateway-api-schema.yaml --output=gateway-api-specs.html`
2. upload generated `gateway-api-specs.html` file to https://github.com/shambupujar/radix-docs/tree/release/static/api-reference
3. Contact with repository owner (Shambu) to deploy them.
