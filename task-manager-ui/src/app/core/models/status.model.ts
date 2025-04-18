export interface Status {
  statusId: number;
  name: string;
  isActive: boolean;
  createdAt: Date;
  createdBy: number;
  updatedAt: Date;
  updatedBy: number;
  order: number;
}

export interface CreateStatusRequest {
  name: string;
  isActive: boolean;
}

export interface UpdateStatusRequest {
  name: string;
  isActive: boolean;
}
