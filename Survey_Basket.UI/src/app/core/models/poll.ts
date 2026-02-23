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

export interface PollStatsResponse {
  totalPolls: number;
  activePolls: number;
  draftPolls: number;
  votesCount: number;
  answersCount: number;
}
