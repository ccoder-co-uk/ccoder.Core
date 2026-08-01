# OpenAPI response contract policy

Swagger describes the responses of the business operations. OData `$metadata` remains a read-only discovery endpoint and is not modified by this policy.

## Adopted conventions

- Standard OData `Post` CRUD actions advertise `201 Created`.
- Standard OData `Delete` CRUD actions advertise `204 No Content`.
- Reads, updates, OData actions and OData functions preserve their declared `200 OK` response unless the action declares a more specific response.
- Keyed operations advertise `404 Not Found`.
- Request-body operations advertise the framework-level `400 Bad Request` and `415 Unsupported Media Type` responses.
- Endpoints protected by ASP.NET authorization metadata advertise `401 Unauthorized` and `403 Forbidden`.
- Controller catches add the matching response for Validation (`400`), Authentication (`401`), Authorization or Security (`403`), Concurrency or Conflict (`409`), Precondition or ETag (`412`), and UnsupportedMedia (`415`) exception types.
- All documented operations advertise `500 Internal Server Error`, matching the terminal server-failure policy.
- Explicit response metadata is preserved. OData bound and unbound actions are not treated as entity creation merely because they use `POST`.

The implementation is a shared Core Swagger operation filter. This keeps aggregate documentation consistent without adding response attributes to every controller action. The CodeAnalysis RFC rules remain the source-level enforcement for the actual controller response paths; the filter does not change runtime status codes.

## Verification snapshot

The local package-mode Core profile enabled nine domain Swagger documents plus Core. It exposed 353 operations in total:

- 353 of 353 operations advertised `500`.
- 39 standard OData creates advertised `201`.
- 34 standard OData deletes advertised `204`.
- No operation advertised only `200`.
- AppSecurity, ContentManagement, DocumentManagement, Logging, Mail, Packaging, Security and Workflow `$metadata` endpoints continued to return `200`.
- AI correctly had no OData metadata document in this profile.

CRM was not configured in this local profile, so it was intentionally absent under the optional-domain composition policy. The full aggregate inventory should be regenerated from the deployment profile after the next Core package is consumed.
