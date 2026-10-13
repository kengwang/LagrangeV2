# Nested forwarded messages

QQ's nested-forward payload is a graph of named items inside one
`PbMultiMsgTransmit`. `MultiMsg` is the root. Each nested card's XML
`m_fileName` (or LightApp `extra.filename` / `detail.uniseq`) must name another
item containing that card's messages. A downloadable `m_resid` alone is not
enough for client-side expansion.

Core builds nested resources bottom-up for the final recipient and includes
all descendants in every containing resource. Filenames are unique within an
upload, and repeated references to the same entity share their prepared item.
Forwarded nodes use type 9 and keep the sender snapshot. Text and Markdown
use their existing protocol elements; this does not add media-upload support.

Receiving indexes all items and resolves embedded names before downloading
independent resource IDs. Downloads are cached only for the current parsing
scope. Missing roots, duplicate names and cyclic item/resource references are
reported as errors. An embedded empty item is marked loaded and is not fetched
again when reforwarded.

`NestedForwardTest` contains a minimized, synthetic version of captured message
1289: two root nodes and a named child with two text messages. No real identities,
resource IDs, avatars or authentication data are included. Core tests cover
graph construction, resolution and reforwarding; `NestedForwardSendTests` exercises
the Milky segment conversion and group-send handler with an offline transport.

Online checks must inspect both the uploaded/downloaded item graph and actual
QQ client expansion. A successful send response or group-history entry alone
does not establish that a nested card can be expanded.

On 2026-10-13, the corrected two-layer text and three-layer text messages
(sequences 1290 and 1291 in the authorized test group) were downloaded and
validated, and the user confirmed that both could be expanded in QQ. The
two-layer Markdown resource also retained both items and its Markdown content,
but QQ returned success with no message sequence for the final send, and it
was absent from group history. A later retry and a sender-UID probe had the
same result. Markdown client-display acceptance remains unverified; no cause
is assigned to this separate server behavior.
