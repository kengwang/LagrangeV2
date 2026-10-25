using FastEndpoints;
using FastEndpoints.Mcp;
using System.Text.Json;

namespace Lagrange.Milky.Mcp;

public static class MilkyMcpTools
{
    private static readonly Dictionary<string, string[]> RequiredArguments = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, ToolMetadata> Tools = new(StringComparer.Ordinal)
    {
        { "commit_flash_file", Write("Commit an uploaded flash-transfer file using fileset_uuid, upload_key, file_uuid, file_name and file_size.") },
        { "complete_flash_task", Write("Complete the flash-transfer upload task identified by fileset_uuid.") },
        { "convert_record", Read("Convert audio from file_uri to pcm, wav or silk and return inline Base64 bytes. The default format is wav.") },
        { "create_flash_task", Write("Create a flash-transfer upload task for file_name, file_size and optional file_type; returns upload credentials.") },
        { "create_group_folder", Write("Create a shared-file folder named folder_name in group_id.") },
        { "delete_flash_file", Write("Delete the flash-transfer fileset identified by fileset_uuid.", destructive: true) },
        { "delete_group_file", Write("Delete file_id from the shared files of group_id.", destructive: true) },
        { "delete_group_folder", Write("Delete folder_id from the shared files of group_id.", destructive: true) },
        { "upload_flash_files", Write("Upload files containing file_uri and file_name to a new flash-transfer fileset, optionally named name; returns sharing metadata.") },
        { "get_flash_download", Read("Get download metadata for fileset_uuid and optional one-based file_index; does not download the file content.") },
        { "get_flash_share_link", Read("Get the official sharing URL for fileset_uuid.") },
        { "resolve_flash_share_code", Read("Resolve a QQ flash-transfer share_code or official sharing URL to its fileset_uuid.") },
        { "get_download_rkeys", Read("Request the authenticated account's rich-media download RKeys. The result contains sensitive access credentials.") },
        { "get_flash_fileset", Read("Get metadata for the flash-transfer fileset identified by fileset_uuid.") },
        { "get_flash_file_url", Read("Get a download URL for fileset_uuid and optional file_id.") },
        { "get_group_file_count", Read("Get the shared-file count for group_id.") },
        { "get_group_file_download_url", Read("Get a download URL for file_id in group_id.") },
        { "get_group_file_space", Read("Get used and total shared-file storage space for group_id.") },
        { "get_group_file_url", Read("Resolve the group file identified by group_id and file_id to its URL.") },
        { "get_group_files", Read("List shared files and folders in group_id and optional parent_folder_id, defaulting to the root folder.") },
        { "get_group_ptt_url", Read("Resolve a group voice-message resource_id to a temporary download URL.") },
        { "get_group_video_url", Read("Resolve a group video resource_id to a temporary download URL.") },
        { "get_private_ptt_url", Read("Resolve a private voice-message resource_id to a temporary download URL.") },
        { "get_private_video_url", Read("Resolve a private video resource_id to a temporary download URL.") },
        { "get_private_file_download_url", Read("Resolve the private file identified by file_id to a download URL.") },
        { "list_flash_filesets", Read("List the authenticated account's flash-transfer filesets, up to limit entries.") },
        { "move_group_file", Write("Move file_id within group_id from parent_folder_id to target_folder_id.", idempotent: true) },
        { "publish_group_file", Write("Publish the existing shared file identified by group_id and file_id.", destructive: true) },
        { "rename_flash_file", Write("Rename fileset_uuid to new_name.", idempotent: true) },
        { "rename_group_file", Write("Rename file_id in group_id and parent_directory to new_file_name.", idempotent: true) },
        { "rename_group_folder", Write("Rename folder_id in group_id to new_folder_name.", idempotent: true) },
        { "send_flash_message", Write("Send the flash-transfer fileset identified by fileset_uuid to user_id or group_id. Repeated calls send additional messages.", destructive: true) },
        { "set_flash_task_status", Write("Set the status of the flash-transfer task identified by fileset_uuid.", idempotent: true) },
        { "trans_group_file", Write("Transfer the group file identified by group_id and file_id to QQ's saved-file service; returns its saved-file identifiers.") },
        { "upload_group_file", Write("Upload file_uri as file_name to group_id and optional parent_folder_id, creating a shared group file.") },
        { "upload_private_file", Write("Upload file_uri as file_name and send the file to user_id. Repeated calls send additional files.", destructive: true) },

        { "delete_friend", Write("Delete user_id from the friend list and optionally block that account using block.", destructive: true) },
        { "get_friend_requests", Read("List the authenticated account's friend requests, including their source, comment and state.") },
        { "get_unidirectional_friend_list", Read("List unidirectional friend relationships for the authenticated account.") },
        { "handle_friend_request", Write("Accept or reject the friend request identified by flag using approve.", destructive: true) },
        { "send_friend_nudge", Write("Nudge user_id in a private conversation; is_self selects a nudge of the authenticated account. Repeated calls send additional nudges.", destructive: true) },
        { "set_friend_category", Write("Move user_id to the friend category identified by category_id.", idempotent: true) },
        { "set_friend_remark", Write("Set the authenticated account's remark for user_id to remark.", idempotent: true) },

        { "accept_group_invitation", Write("Accept the invitation identified by group_id and invitation_seq, joining the invited group.", destructive: true) },
        { "accept_group_request", Write("Approve the group request identified by notification_seq, notification_type and group_id; is_filtered selects the filtered request list.", destructive: true) },
        { "cancel_group_todo", Write("Remove a group to-do in group_id; message_seq selects a specific to-do when supplied.", destructive: true) },
        { "comment_group_album_media", Write("Publish content as a comment on media_id in album_id and group_id. Repeated calls create additional comments.", destructive: true) },
        { "complete_group_todo", Write("Mark a group to-do in group_id as completed; message_seq selects a specific to-do when supplied.", idempotent: true) },
        { "delete_group_album_media", Write("Delete media_id from album_id in group_id, with optional batch_id.", destructive: true) },
        { "delete_group_announcement", Write("Delete announcement_id from group_id.", destructive: true) },
        { "get_group_admin_settings", Read("Get join, invitation, history visibility and robot settings for group_id. The response may contain the group's join answer.") },
        { "get_group_album_list", Read("List albums for group_id; attach_info continues a previous page.") },
        { "get_group_album_media", Read("List media in album_id for group_id; attach_info supplies the pagination cursor.") },
        { "get_group_announcements", Read("List announcements in group_id using start and count pagination.") },
        { "get_group_at_all_remain", Read("Get whether the authenticated account can mention everyone in group_id and the remaining mention quotas.") },
        { "get_group_essence", Read("List highlighted messages in group_id using page_start and page_limit pagination.") },
        { "get_group_essence_messages", Read("List highlighted messages in group_id, including sender and highlighting metadata, using page_start and page_limit.") },
        { "get_group_honor", Read("Get members holding honor_type in group_id.") },
        { "get_group_notifications", Read("List group notifications using start_notification_seq, limit and is_filtered; returns the next notification cursor.") },
        { "get_group_sign_in", Read("Get group_id sign-in records for optional day.") },
        { "get_group_todo", Read("Get the current group to-do and its source message for group_id.") },
        { "get_group_todo_list", Read("List group to-dos and their source message sequences for group_id.") },
        { "kick_group_member", Write("Remove user_id from group_id; reject_add_request optionally prevents the removed account from requesting to rejoin.", destructive: true) },
        { "quit_group", Write("Leave group_id as the authenticated account.", destructive: true) },
        { "reject_group_invitation", Write("Reject the invitation identified by group_id and invitation_seq.", destructive: true) },
        { "reject_group_request", Write("Reject the group request identified by notification_seq, notification_type and group_id, with optional reason and is_filtered.", destructive: true) },
        { "send_group_announcement", Write("Publish content as an announcement in group_id, with pinning, notification and optional uploaded-picture settings.", destructive: true) },
        { "send_group_message_reaction", Write("Add or remove reaction on message_seq in group_id using is_add.", idempotent: true) },
        { "send_group_nudge", Write("Nudge user_id in group_id. Repeated calls send additional nudges.", destructive: true) },
        { "send_group_sign", Write("Perform the authenticated account's daily sign-in for group_id and return sign-in details.") },
        { "set_group_add_option", Write("Set the join policy add_type for group_id, with optional group_question and group_answer.", idempotent: true) },
        { "set_group_album_like", Write("Set or remove a like using is_like on album_id, batch_id and optional media_id in group_id.", idempotent: true) },
        { "set_group_avatar", Write("Replace the avatar of group_id with the resource at image_uri.", idempotent: true) },
        { "set_group_essence_message", Write("Mark message_seq in group_id as a highlighted message.", idempotent: true) },
        { "set_group_member_invite_policy", Write("Set group_id member invitation policy using policy and current_privilege_flag.", idempotent: true) },
        { "set_group_member_admin", Write("Grant or remove administrator status for user_id in group_id using is_set.", idempotent: true) },
        { "set_group_member_card", Write("Replace user_id's group card in group_id with card.", idempotent: true) },
        { "set_group_member_mute", Write("Mute user_id in group_id for duration seconds; duration zero removes the mute. Repeated calls can change the mute expiry.") },
        { "set_group_member_permission", Write("Set whether permission is allowed in group_id using allow and current_privilege_flag.", idempotent: true) },
        { "set_group_member_special_title", Write("Set user_id's special_title in group_id.", idempotent: true) },
        { "set_group_name", Write("Rename group_id to new_group_name.", idempotent: true) },
        { "set_group_new_member_history_visibility", Write("Set whether new members can read group_id history using visible.", idempotent: true) },
        { "set_group_remark", Write("Set the authenticated account's remark for group_id to remark.", idempotent: true) },
        { "set_group_robot_add_option", Write("Set robot_member_switch and robot_member_examine policies for group_id.", idempotent: true) },
        { "set_group_search", Write("Change group_id discoverability using no_finger_open and no_code_finger_open.", idempotent: true) },
        { "set_group_todo", Write("Create a group to-do from message_seq in group_id.") },
        { "set_group_whole_mute", Write("Enable or disable the whole-group mute for group_id using is_mute.", idempotent: true) },
        { "transfer_group", Write("Transfer ownership of group_id to user_id. This changes the group's owner.", destructive: true) },
        { "upload_group_album_image", Write("Upload image_uri to album_id in group_id, with optional album_name and file_name; creates new album media.") },
        { "upload_group_album_video", Write("Upload video_uri and cover_uri to album_id in group_id, with optional duration_ms, album_name and file_name; creates new album media.") },
        { "upload_group_announcement_image", Write("Upload image_uri for use in a group announcement; returns picture identifiers and dimensions.") },

        { "click_inline_keyboard_button", Write("Invoke button_id and callback_data on the inline keyboard for app_id and message_seq in group_id. The remote callback may perform actions.", destructive: true) },
        { "get_emoji_likes", Read("List users who reacted with code to message_seq in group_id, using cookie and count pagination.") },
        { "get_group_reaction_summary", Read("Get reaction codes, counts and timestamps for message_seq in group_id.") },
        { "send_like", Write("Send count profile likes to user_id. Repeated calls send additional likes.", destructive: true) },
        { "send_poke", Write("Send a nudge in message_scene to peer_id, optionally targeting target_user_id. Repeated calls send additional nudges.", destructive: true) },
        { "set_group_reaction", Write("Set or remove reaction code on message_seq in group_id using is_set.", idempotent: true) },
        { "set_input_status", Write("Send typing/input event_type to user_id; this changes the status visible to that account.") },

        { "get_ai_voice_list", Read("List available AI voices for group_id and optional chat_type.") },
        { "get_collection", Read("Get up to count items from the authenticated account's saved-message collection.") },
        { "get_forwarded_messages", Read("Retrieve the forwarded-message bundle identified by resource_id, including its message nodes and segments.") },
        { "get_history_messages", Read("Get messages for message_scene and peer_id, with optional start_message_seq and limit.") },
        { "get_message", Read("Get message_seq from message_scene and peer_id, including its sender and message segments.") },
        { "get_resource_temp_url", Read("Get a temporary rich-media download URL for resource_id.") },
        { "probe_history_sync_state", Read("Inspect read and latest history positions for group_ids and private_targets containing user_id and uid.") },
        { "get_history_sync_page", Read("Read a forward history page for message_scene and peer_id from next_seq through latest_seq, bounded by limit.") },
        { "get_private_history_roam_page", Read("Read a private roaming history page for user_id using time and random cursors, bounded by limit.") },
        { "mark_all_read", Write("Mark all supported conversations as read for the authenticated account.", idempotent: true) },
        { "mark_message_as_read", Write("Mark messages through last_read_seq as read in the conversation selected by message_scene and peer_id.", idempotent: true) },
        { "recall_group_message", Write("Recall message_seq in group_id, removing the message from the conversation when QQ permits it.", destructive: true) },
        { "recall_private_message", Write("Recall message_seq in the private conversation with user_id when QQ permits it.", destructive: true) },
        { "send_ai_voice", Write("Synthesize and send text using voice_id to group_id, with optional chat_type. Repeated calls send additional voice messages.", destructive: true) },
        { "send_group_message", Write("Send message segments to group_id, with optional message_style. Segments may include text, mentions, media and forwarded nodes. Repeated calls send additional messages.", destructive: true) },
        { "send_private_message", Write("Send message segments to user_id, with optional message_style. Repeated calls send additional private messages.", destructive: true) },
        { "send_temp_message", Write("Send message segments to user_id through group_id as a temporary conversation, with optional message_style. Repeated calls send additional messages.", destructive: true) },
        { "synthesize_ai_voice", Write("Synthesize text with voice_id for group_id and optional chat_type; creates a voice resource and returns its URL and metadata.") },
        { "transcribe_voice", Read("Transcribe the voice message selected by message_scene, peer_id and message_seq into text.") },

        { "add_custom_face", Write("Upload image_uri and add it to the authenticated account's custom emoticons.") },
        { "comment_qzone_message", Write("Publish content as a comment on message_id belonging to target_uin. Repeated calls create additional comments.", destructive: true) },
        { "delete_custom_face", Write("Delete face_id from the authenticated account's custom emoticons.", destructive: true) },
        { "delete_qzone_message", Write("Delete the authenticated account's Qzone post identified by message_id.", destructive: true) },
        { "get_group_system_msg", Read("Get group notifications for group_id from the latest 100 notifications.") },
        { "get_group_shut_list", Read("List currently muted members of group_id and their mute expiry times.") },
        { "get_doubt_friends_add_request", Read("List suspicious or filtered friend requests for the authenticated account.") },
        { "set_doubt_friends_add_request", Write("Accept or reject the suspicious friend request identified by request_id using approve.", destructive: true) },
        { "get_qidian_corp_info", Read("Get enterprise/Qidian profile information for user_id.") },
        { "send_ark_share", Read("Generate a sharing Ark card for group_id or user_id and return its payload.") },
        { "send_tuwen_ark", Write("Send a rich link card to peer_id using title, description, summary, url and preview_url; supplying group_id selects a group conversation.", destructive: true) },
        { "get_cookies", Read("Request authenticated QQ cookies for domains, defaulting to qq.com. The response contains sensitive account credentials.") },
        { "get_csrf_token", Read("Request the authenticated account's CSRF token. The response contains a sensitive account credential.") },
        { "get_custom_face_detail", Read("Get descriptions for custom emoticon entries containing face_id and md5.") },
        { "get_custom_face_list", Read("List the authenticated account's custom emoticon identifiers and total count.") },
        { "get_custom_face_url_list", Read("List the authenticated account's custom emoticon identifiers and image URLs.") },
        { "get_friend_dress", Read("Get avatar decoration and membership information for user_id or target_uin.") },
        { "get_friend_info", Read("Get the friend profile for user_id; no_cache requests a refresh from QQ.") },
        { "get_friend_list", Read("List friends and their categories; no_cache requests a refresh from QQ.") },
        { "get_friends_status", Read("Query the authenticated account's current friend-status response, including raw status fields. The response may contain only part of the friend list.") },
        { "get_group_ext_info", Read("Get extended metadata for group_id, including the latest message sequence.") },
        { "get_group_info", Read("Get group_id information; no_cache requests a refresh from QQ.") },
        { "get_group_list", Read("List groups joined by the authenticated account; no_cache requests a refresh from QQ.") },
        { "get_group_member_info", Read("Get user_id's membership profile in group_id; no_cache requests a refresh from QQ.") },
        { "get_group_member_list", Read("List members of group_id; no_cache requests a refresh from QQ.") },
        { "get_impl_info", Read("Get implementation, Milky and QQ protocol versions from the running application.", openWorld: false) },
        { "get_login_info", Read("Get the running account's numeric UIN and currently known nickname.", openWorld: false) },
        { "get_mini_app_ark", Read("Generate a mini-app Ark payload using type, title, description, picture_url and jump_url.") },
        { "get_online_devices", Read("Get devices observed for the authenticated account in the current session; observed=false means no device list has been received.", openWorld: false) },
        { "get_peer_pins", Read("List the authenticated account's pinned conversation identifiers.") },
        { "get_like", Read("Get profile-like information for optional user_id using start and limit pagination.") },
        { "get_qzone_feeds", Read("Read the authenticated account's Qzone feed using page_num or position and count pagination.") },
        { "get_qzone_msg_list", Read("List Qzone posts for optional target_uin using position and count pagination.") },
        { "get_system_faces", Read("List QQ system emoticon packs; refresh requests updated metadata.") },
        { "get_user_profile", Read("Get the public profile of user_id, including nickname, age, location and biography.") },
        { "get_user_status", Read("Get QQ online and extended status values for user_id.") },
        { "image_ocr", Read("Recognize text in image_url using QQ's OCR service; returns text, confidence and coordinates.") },
        { "modify_custom_face", Write("Replace the description of custom face_id, supplying its md5.", idempotent: true) },
        { "move_custom_face", Write("Move face_id to position using the supplied entries of face_id and md5.", idempotent: true) },
        { "publish_qzone_message", Write("Publish content and optional rich_value as a new Qzone post. Repeated calls create additional posts.", destructive: true) },
        { "request_database_key", Read("Request the authenticated account's QQ database key for db_salt. The response contains sensitive decryption credentials.") },
        { "request_decrypt_key", Write("Request the authenticated account's QQ database password: mode=encrypt creates key metadata; mode=decrypt requires key_meta containing 128 hexadecimal characters. Returns sensitive credentials.") },
        { "search_system_faces", Read("Search system emoticons by query; refresh requests updated metadata.") },
        { "send_packet", Write("Send arbitrary SSO command cmd with hexadecimal payload data through the authenticated QQ connection. rsp selects whether to await a reply. Commands may change account or server state.", destructive: true) },
        { "send_profile_like", Write("Send count profile likes to user_id. Repeated calls send additional likes.", destructive: true) },
        { "set_avatar", Write("Replace the authenticated account's avatar with the image resource identified by uri.", idempotent: true) },
        { "set_bio", Write("Replace the authenticated account's biography with bio.", idempotent: true) },
        { "set_diy_online_status", Write("Set the authenticated account's custom online status using face_id and text.", idempotent: true) },
        { "set_nickname", Write("Replace the authenticated account's nickname with nickname.", idempotent: true) },
        { "set_online_status", Write("Set the authenticated account's QQ online status to status.", idempotent: true) },
        { "set_peer_pin", Write("Pin or unpin peer_id in message_scene using is_pinned.", idempotent: true) },
        { "set_profile", Write("Update the authenticated account's nickname, personal_note and sex when supplied.", idempotent: true) },
        { "set_qzone_black", Write("Add or remove target_uin from the Qzone blacklist using ban.", idempotent: true) },
        { "set_qzone_like", Write("Set or remove a Qzone post like using target_uin, message_id, like and abstime.", idempotent: true) },
        { "set_qzone_message_visibility", Write("Change message_id Qzone visibility using right and the selected users.", idempotent: true) },
        { "set_self_long_nick", Write("Replace the authenticated account's long signature with long_nick.", idempotent: true) },
        { "translate_en_to_zh", Read("Translate the English strings in words into Chinese using QQ's translation service.") },
        { "upload_qzone_image", Write("Upload image_uri to Qzone and return image metadata and rich_value for use in a post.") },
    };

    public static IReadOnlyCollection<string> Names => Tools.Keys;

    static MilkyMcpTools()
    {
        foreach (var (name, metadata) in Tools)
        {
            if (name.Length is < 1 or > 64 || name.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-'))
                throw new InvalidOperationException($"Invalid Milky MCP tool name: {name}.");
            if (string.IsNullOrWhiteSpace(metadata.Description))
                throw new InvalidOperationException($"Milky MCP tool {name} has no description.");
        }
    }

    public static void Configure(EndpointDefinition definition, JsonSerializerOptions serializerOptions)
    {
        var routes = definition.Routes.Where(route => route.StartsWith("/api/", StringComparison.Ordinal)).ToArray();
        if (routes.Length == 0)
            return;
        if (routes.Length != 1)
            throw new InvalidOperationException($"Milky MCP requires one API route per endpoint: {definition.EndpointType.FullName}.");

        string name = routes[0]["/api/".Length..];
        if (!Tools.TryGetValue(name, out var metadata))
            throw new InvalidOperationException($"Milky API {routes[0]} has no MCP tool metadata.");

        RequiredArguments[name] = definition.ReqDtoType == typeof(EmptyRequest)
            ? []
            : serializerOptions.GetTypeInfo(definition.ReqDtoType).Properties
                .Where(property => property.IsRequired).Select(property => property.Name).ToArray();

        definition.McpTool(name, metadata.Description, tool =>
        {
            tool.Hints.ReadOnly = metadata.ReadOnly;
            tool.Hints.Destructive = metadata.Destructive;
            tool.Hints.Idempotent = metadata.Idempotent;
            tool.Hints.OpenWorld = metadata.OpenWorld;
        });
    }

    internal static string[] GetRequiredArguments(string name)
        => RequiredArguments.TryGetValue(name, out var arguments) ? arguments : [];

    private static ToolMetadata Read(string description, bool openWorld = true)
        => new(description, true, false, true, openWorld);

    private static ToolMetadata Write(string description, bool idempotent = false, bool destructive = false)
        => new(description, false, destructive, idempotent, true);

    private sealed record ToolMetadata(string Description, bool ReadOnly, bool Destructive, bool Idempotent, bool OpenWorld);
}
