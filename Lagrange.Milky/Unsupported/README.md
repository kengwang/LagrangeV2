# Unsupported Milky 1.2 APIs

The following Milky APIs are intentionally not registered because this tree does not currently have a reliable LagrangeV2-compatible protocol implementation to call. They should not be implemented with guessed packets or empty placeholder responses.

- `set_nickname`
- `set_bio`
- `get_group_announcements`
- `send_group_announcement`
- `delete_group_announcement`
- `get_group_essence_messages`

The group announcement APIs and `get_group_essence_messages` have OneBot-side references in `E:\Clones\Lagrange.Core`, but those references use web endpoints through `TicketService`/cookies rather than a Core protocol service that is currently present in LagrangeV2. They remain unregistered until that HTTP ticket capability is ported deliberately.
