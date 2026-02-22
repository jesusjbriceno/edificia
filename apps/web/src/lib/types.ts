// ─── Enums ────────────────────────────────────────────────

export const InterventionType = {
  NewConstruction: 0,
  Reform: 1,
  Extension: 2,
} as const;
export type InterventionType = (typeof InterventionType)[keyof typeof InterventionType];

export const InterventionTypeLabels: Record<InterventionType, string> = {
  [InterventionType.NewConstruction]: 'Obra Nueva',
  [InterventionType.Reform]: 'Reforma',
  [InterventionType.Extension]: 'Ampliación',
};

/** Maps API string enum values to Spanish labels. */
export const InterventionTypeStringLabels: Record<string, string> = {
  NewConstruction: 'Obra Nueva',
  Reform: 'Reforma',
  Extension: 'Ampliación',
};

export const ProjectStatus = {
  Draft: 'Draft',
  InProgress: 'InProgress',
  Completed: 'Completed',
  Archived: 'Archived',
  PendingReview: 'PendingReview',
} as const;
export type ProjectStatus = (typeof ProjectStatus)[keyof typeof ProjectStatus];

export const ProjectStatusLabels: Record<ProjectStatus, string> = {
  [ProjectStatus.Draft]: 'Borrador',
  [ProjectStatus.InProgress]: 'En redacción',
  [ProjectStatus.Completed]: 'Completado',
  [ProjectStatus.Archived]: 'Archivado',
  [ProjectStatus.PendingReview]: 'Pendiente de revisión',
};

export const Role = {
  Root: 'Root',
  Admin: 'Admin',
  Architect: 'Architect',
  Collaborator: 'Collaborator',
} as const;
export type Role = (typeof Role)[keyof typeof Role];

export const RoleLabels: Record<Role, string> = {
  [Role.Root]: 'Super Administrador',
  [Role.Admin]: 'Administrador',
  [Role.Architect]: 'Arquitecto',
  [Role.Collaborator]: 'Colaborador',
};

// ─── Shared ──────────────────────────────────────────────

export interface ProblemDetails {
  status: number;
  title: string;
  detail?: string;
  code?: string;
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors: { property: string; message: string }[];
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// ─── Auth ────────────────────────────────────────────────

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string | null;
  expiresInMinutes: number;
  mustChangePassword: boolean;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  collegiateNumber: string | null;
  roles: Role[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RevokeTokenRequest {
  refreshToken: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface CurrentUserResponse {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface UpdateProfileRequest {
  fullName: string;
  collegiateNumber?: string | null;
}

export interface UpdateProfileResponse {
  id: string;
  email: string;
  fullName: string;
  collegiateNumber: string | null;
}

export interface ForgotPasswordRequest {
  email: string;
}

// ─── Projects ────────────────────────────────────────────

export interface ProjectResponse {
  id: string;
  title: string;
  description: string | null;
  address: string | null;
  interventionType: string;
  isLoeRequired: boolean;
  cadastralReference: string | null;
  localRegulations: string | null;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateProjectRequest {
  title: string;
  interventionType: InterventionType;
  isLoeRequired: boolean;
  description?: string | null;
  address?: string | null;
  cadastralReference?: string | null;
  localRegulations?: string | null;
}

export interface ContentTreeResponse {
  projectId: string;
  interventionType: string;
  isLoeRequired: boolean;
  contentTreeJson: string | null;
}

export interface UpdateProjectTreeRequest {
  contentTreeJson: string;
}

export interface UpdateSectionRequest {
  content: string;
}

// ─── Content Tree Nodes ──────────────────────────────────

/** A node in the CTE normative content tree (recursive). */
export interface ContentTreeNode {
  id: string;
  title: string;
  requiresNewWork: boolean;
  content: string | null;
  sections: ContentTreeNode[];
}

/** Root shape of the content tree JSON stored in the DB / cte_2024.json. */
export interface ContentTree {
  chapters: ContentTreeNode[];
}

/** Configuration for tree filtering based on project strategy. */
export interface TreeFilterConfig {
  interventionType: InterventionType;
  isLoeRequired: boolean;
}

// ─── Users ───────────────────────────────────────────────

export interface UserResponse {
  id: string;
  email: string;
  fullName: string;
  collegiateNumber: string | null;
  isActive: boolean;
  mustChangePassword: boolean;
  role: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
  role: string;
  collegiateNumber?: string | null;
}

export interface UpdateUserRequest {
  fullName: string;
  role: string;
  collegiateNumber?: string | null;
}

// ─── AI ──────────────────────────────────────────────────

export interface GenerateTextRequest {
  sectionId: string;
  prompt: string;
  context?: string | null;
}

export interface GeneratedTextResponse {
  projectId: string;
  sectionId: string;
  generatedText: string;
}
