# Aggregate API inventory — 2026-08-02

This capture was generated from an isolated local aggregate consuming the published `cCoder.Core` package `2026.8.1.2321`. CRM was explicitly configured. Swagger was explicitly enabled for inventory generation because production suppresses it by policy. No production endpoint or database was used.

## Aggregate surface

- 11 Swagger documents returned `200`: Core plus ten configured child contexts.
- 576 operations were advertised, comprising 576 unique case-insensitive path and verb pairs.
- No operation was duplicated across Swagger documents.
- CRM was present as `ClientRelationshipManagement`, with 229 Swagger operations, 32 OData entity sets and a `200` metadata response.
- Nine OData metadata documents returned `200`, describing 80 entity sets in total.
- Packaging metadata returned `200` and described two entity sets.
- AI metadata returned `404` as expected because AI is not an OData context.
- Core metadata returned `404` as expected because Core owns no OData context.

## OpenAPI response contracts

All 576 operations advertise `500 Internal Server Error`, and no operation advertises only `200 OK`.

| Response | Operations |
| --- | ---: |
| `200` | 439 |
| `201` | 71 |
| `204` | 66 |
| `400` | 282 |
| `401` | 0 |
| `403` | 85 |
| `404` | 284 |
| `409` | 0 |
| `412` | 0 |
| `415` | 230 |
| `500` | 576 |

The 71 documented `201` responses are standard OData creates. The 66 documented `204` responses are standard OData deletes. Other POST operations retain their operation-specific success response rather than being incorrectly classified as creates.

## Observations requiring policy confirmation

The aggregate currently advertises no `401`, `409` or `412` responses. This is not evidence that every operation requires those responses: `401` is reserved for authentication failures, while `409` and `412` require concurrency or precondition semantics. It does mean the documentation filter found no applicable ASP.NET authorization metadata or directly caught exception type in the published surface. These zero counts should be compared with runtime acceptance tests and the CodeAnalysis call-chain model before being accepted as intentional.

The workbook's OData candidate rows are convention-derived planning entries. They do not prove that every candidate verb is implemented.

## Reproduction

The workbook was captured at `2026-08-02T06:46:49.938572+00:00`. The generator removes stale API-root, Swagger and OData captures before every run. Capture provenance, context totals and response coverage are recorded in the workbook's `Contract Summary`, `API Contexts` and `Source Documents` sheets.
