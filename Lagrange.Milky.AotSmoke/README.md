# Milky request-binding smoke

This console executable initializes FastEndpoints with the production binding configuration, then binds real Milky request DTOs from in-memory HTTP requests. It never starts a listener, creates a bot session, or calls QQ. Failure exits nonzero.

The scenarios cover group/private sends, polymorphic and string collections, strings, nullable numeric values, booleans, unsigned values, nullable dates, and rejection of malformed numeric query values. Query keys use the DTO's CLR property names; JSON uses its declared snake_case names.

Run locally with `dotnet run --project Lagrange.Milky.AotSmoke -c Release`. For actual AOT evidence, publish for the target RID and run the published executable directly with `--require-native`. The printed dynamic-code feature flag alone is not proof of native compilation: SDK runtime settings can disable dynamic code for a managed executable too.

## Negative regression variant

`-p:ReproduceUncachedBinding=true` compiles a smoke-only branch that registers generated JSON metadata but omits production binder metadata and scalar parsers. Publish it into separate output and intermediate directories, then run that executable. A binding/type-initializer failure is the expected regression result. This switch does not modify production Milky code.

Keep normal and negative publishes isolated (including intermediates), and do not interpret a managed run as a trimming regression check. The reproduction branch is excluded from normal builds.
