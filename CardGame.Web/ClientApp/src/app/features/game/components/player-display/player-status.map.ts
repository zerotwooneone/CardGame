// --- Added Exported Map ---
import {PlayerStatus} from "./player.status";

export const PlayerStatusMap: { [key in PlayerStatus]: string } = {
  [PlayerStatus.Unknown]: 'Unknown',
  [PlayerStatus.Active]: 'Active',
  [PlayerStatus.Eliminated]: 'Eliminated'
};
