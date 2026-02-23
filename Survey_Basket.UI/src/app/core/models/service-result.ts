export interface ServiceError {
  description: string;
  code: number;
}

export interface ServiceResult<T> {
  succeeded: boolean;
  data?: T;
  error?: ServiceError;
}

export interface ServiceListResult<TItem, TStats> {
  items: PagedList<TItem>;
  stats: TStats;
}

export interface PagedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
