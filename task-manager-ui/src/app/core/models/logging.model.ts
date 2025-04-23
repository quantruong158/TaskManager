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

export interface ActivityLog {
  logId: number;
  user: UserLog;
  action: string;
  targetTable: string;
  targetId: number;
  timestamp: Date;
}

export interface UserLog {
  userId: number;
  name: string;
  email: string;
}
