<div align="center">

![Lagrange.Milky](./Resources/banner.svg)

_[Milky](https://github.com/SaltifyDev/milky) protocol implementation based on [Lagrange.Core V2](https://github.com/LagrangeDev/LagrangeV2)_

</div>

## Document

https://lagrangedev.github.io/Lagrange.Milky.Document

## Contribute

### api

Write an implementation of `IApiHandler<TRequest, TResult>`/`INoRequestApiHandler<TResult>`/`INoResultApiHandler<TRequest>` in the folder corresponding to the `Lagrange.Milky\Api\Handlers` category, and add `TRequest` and `TResult` to the collection of `[JsonSerializable]` attributes above `JsonContext` in `Lagrange.Milky\Serialization\Serializer.cs`.

### event

Write an implementation of `IEventConverter<TEvent, TData>` in `Lagrange.Milky\Events\Converters`, and add `TData` to the `[JsonSerializable]` attributes above `JsonContext` in `Lagrange.Milky\Serialization\Serializer.cs`.

## Feature List

- Api
  - [x] Http
- Event
  - [x] SSE
  - [x] WebSocket
  - [x] WebHook

### api

#### system

[x] get_login_info
[x] get_impl_info
[x] get_user_profile
[x] get_friend_list
[x] get_friend_info
[x] get_group_list
[x] get_group_info
[x] get_group_detail_info
[x] get_group_member_list
[x] get_group_member_info
[x] get_group_ext_info
[x] get_peer_pins (explicit unsupported error when QQ wire endpoint is unavailable)
[x] set_peer_pin
[x] set_avatar
[x] set_nickname
[x] set_bio
[x] set_diy_online_status
[x] get_custom_face_url_list
[x] get_cookies
[x] get_csrf_token

#### message

[x] send_private_message
[x] send_group_message
[x] recall_private_message
[x] recall_group_message
[x] get_message
[x] get_history_messages
  - request
    - [x] start_message_seq - Friend history derives the latest sequence when omitted.
[x] get_resource_temp_url
[x] get_forwarded_messages
[x] mark_message_as_read
[x] mark_all_read

#### friend

[x] send_friend_nudge
[x] send_profile_like
[x] get_like
[x] translate_en_to_zh
[x] image_ocr
[x] delete_friend
[x] get_friend_requests
[x] accept_friend_request
[x] reject_friend_request

#### group

[x] set_group_name
[x] set_group_avatar
[x] set_group_member_card
[x] set_group_member_special_title
[x] set_group_member_admin
[x] set_group_member_mute
[x] set_group_whole_mute
[x] kick_group_member
[x] get_group_announcements
[x] send_group_announcement
[x] delete_group_announcement
[x] upload_group_announcement_image
[x] get_group_honor
[x] get_group_sign_in
[x] get_group_essence_messages
[x] set_group_essence_message
[x] quit_group
[x] send_group_message_reaction
  - result
    - reaction_type
[x] send_group_nudge
[x] get_group_notifications
[x] accept_group_request
[x] reject_group_request
[x] accept_group_invitation
[x] reject_group_invitation
[x] set_group_add_option
[x] set_group_search
[x] set_group_new_member_history_visibility
[x] set_group_new_member_history
[x] set_group_member_invite_policy
[x] set_group_invite_policy
[x] set_group_robot_add_option
[x] get_group_admin_settings

#### file

[x] upload_private_file
  - result
    - [x] file_id
[x] upload_group_file
  - result
    - [x] file_id
[x] get_private_file_download_url
[x] get_group_file_download_url
[x] get_group_files
[x] move_group_file
[x] rename_group_file
[x] delete_group_file
[x] create_group_folder
  - result
    - [x] file_id
[x] rename_group_folder
[x] delete_group_folder

### event

- [x] bot_offline
- [x] message_receive
- [x] message_recall
- [x] peer_pin_change
- [x] friend_request
- [x] group_join_request
- [x] group_invited_join_request
- [x] group_invitation
- [x] friend_nudge
- [x] friend_file_upload
- [x] group_admin_change
- [x] group_essence_message_change
- [x] group_member_increase
- [x] group_member_decrease
- [x] group_name_change
- [x] group_message_reaction
  - [x] reaction_type
- [x] group_mute
- [x] group_whole_mute
- [x] group_nudge
- [x] group_file_upload

### models

- [x] Friend
- [x] FriendCategory
- [x] Group
- [x] GroupMember
- [x] GroupAnnouncement
- [x] GroupFile
- [x] GroupFolder
- [x] FriendRequest
- [x] GroupNotification
- [x] IncomingMessage
  - [x] friend
  - [x] group
- [x] IncomingForwardedMessage
- [x] GroupEssenceMessage
- [x] [IncomingSegment](#imcoming-segment)
- [x] OutgoingForwardedMessage
- [x] [OutgoingSegment](#outgoing-segment)

### imcoming segment

- [x] text
- [x] mention
- [x] mention_all
- [x] face
- [x] reply
- [x] image
- [x] record
- [x] video
- [x] file
- [x] forward
  - [x] title
  - [x] preview
  - [x] summary
- [x] market_face
- [x] light_app
- [x] xml

### outgoing segment

- [x] text
- [x] mention
- [x] mention_all
- [x] face
- [x] reply
- [x] image
- [x] record
- [x] video
- [x] forward
  - [x] title
  - [x] preview
  - [x] summary
  - [x] prompt
- [x] light_app
- [x] market_face
- [x] xml
