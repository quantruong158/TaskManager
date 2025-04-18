export interface TaskStatusLog {
  historyId: number;
  task: {
    taskId: number;
    title: string;
  };
  status: {
    statusId: number;
    statusName: string;
  };
  changedAt: Date;
  changedBy: string;
}

export interface LoginLog {
  logId: number;
  email: string;
  isSuccess: boolean;
  timestamp: Date;
  userAgent: string;
  attemptIp: string;
}
