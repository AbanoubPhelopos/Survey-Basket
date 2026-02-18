export interface Poll {
  id: string;
  title: string;
  summary: string;
  isPublished: boolean;
  startedAt: string;
  endedAt?: string;
}

export interface PollResponse {
  id: string;
  title: string;
  summary: string;
  isPublished: boolean;
  startedAt: string;
  endedAt?: string;
}

export interface CreatePollRequest {
  title: string;
  summary: string;
  isPublished: boolean;
  startedAt: string;
  endedAt?: string;
}

export interface UpdatePollRequest {
  title: string;
  summary: string;
  isPublished: boolean;
  startedAt: string;
  endedAt?: string;
}

export interface RequestFilters {
  pageNumber: number;
  pageSize: number;
  sortColumn?: string;
  sortDirection?: string;
  searchTerm?: string;
}

export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
