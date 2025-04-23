export interface CreateCommentRequest {
  taskId: number;
  content: string;
}

export interface UserComment {
  userId: number;
  name: string;
  email: string;
}

export interface Comment {
  commentId: number;
  taskId: number;
  content: string;
  createdAt: Date;
  updatedAt: Date;
  user: UserComment;
}
