export interface InfiniteRowModelResult<T> {
  rowsThisBlock: T[];
  lastRow?: number;
}