# Appendix B: ARM Templates

## Secured

This template will deploy all resources with private endpoints.  This effectively removes any access to the PaaS services from the internet.

[Secured ARM Template](./../artifacts/template-secure.json)

## Non-Secured

This template will deploy resources using standard deployment where all resources are available from the internet.

[Non-secured ARM Template](./../artifacts/template.json)
