export interface BonusesResponse {
  bonuses: Bonuses[],
  totalCount:number,
}
export interface Bonuses {
  id: number;
  phoneNumber: string;
  name: string;
  comment: string;
  totalCounter: number;
  currentCounter: number;
  setting: number;
  lastUpdate: Date;
  $$index?: number;
}
