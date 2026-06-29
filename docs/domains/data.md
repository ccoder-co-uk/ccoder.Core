# Data

The Data domain provides the shared persistence layer used by the cCoder platform domains.

## What It Provides

- EF Core data context and context factory support.
- Shared entity mappings and database model support for core platform data.
- Common data access infrastructure consumed by content, document, logging, mail, scheduling, workflow, packaging, and app-security domains.

## Why It Matters

Most domain packages depend on the same core database shape. The aggregate package includes the data layer so consumers get a consistent persistence model and service registration path.

