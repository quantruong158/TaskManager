export interface CommentRequest {
  taskId: number;
  content: string;
}

export interface UserCommentDto {
  userId: number;
  name: string;
  email: string;
}

export interface CommentResponse {
  commentId: number;
  taskId: number;
  content: string;
  createdAt: Date;
  updatedAt: Date;
  user: UserCommentDto;
}
