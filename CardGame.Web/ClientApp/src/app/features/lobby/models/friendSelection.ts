import {UserInfo} from "../../../core/models/userInfo";

/**
 * Internal model used within the dialog component for managing friend selections.
 */
export interface FriendSelection extends UserInfo {
  selected: boolean;
}
