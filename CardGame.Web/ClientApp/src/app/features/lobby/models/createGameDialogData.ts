import {UserInfo} from "../../auth/models/userInfo";

/**
 * Data passed *to* the CreateGameDialogComponent.
 */
export interface CreateGameDialogData {
  knownFriends: UserInfo[]; // List of previously known friends to select from
}
