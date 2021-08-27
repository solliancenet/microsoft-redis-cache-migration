# Appendix B: ARM Templates

## Secure

This template will deploy all resources with private endpoints.  This effectively removes any access to the PaaS services from the internet.

[ARM Template](./../artifacts/template-secure.json)

## Non-Secure

This template will deploy resources using standard deployment where all resources are available from the internet.

[ARM Template](./../artifacts/template.json)
