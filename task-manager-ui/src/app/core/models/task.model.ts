export interface Task {
  taskId: number;
  title: string;
  description?: string;
  priority: string;
  status: {
    statusId: number;
    statusName: string;
  };
  assignedTo?: number;
  assigneeName?: string;
  createdAt: Date;
  createdBy?: string;
  updatedAt?: Date;
  updatedBy?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: string;
  statusId?: number;
  assignedTo?: number;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  priority: string;
  assignedTo?: number;
}

export interface ChangeTaskStatusRequest {
  newStatusId: number;
}

export interface TaskCountResponse {
  totalCount: number;
  taskCounts: {
    statusId: number;
    statusName: string;
    taskCount: number;
  }[];
}
