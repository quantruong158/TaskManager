export interface Role {
  roleId: number;
  roleName: string;
}

export interface User {
  userId: number;
  email: string;
  name: string;
  isActive: boolean;
  roles: Role[];
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  name: string;
  roleIds: number[];
}

export interface UpdateUserRequest {
  email: string;
  name: string;
  roleIds: number[];
  isActive: boolean;
}
