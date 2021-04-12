import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { InfiniteRowModelResult } from './infinite-row-model-result.model';
import { User } from './user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private httpClient: HttpClient, @Inject('BASE_URL') private baseUrl: string) { }

  getUsers(query: string): Observable<InfiniteRowModelResult<User>> {
    return this.httpClient.get<InfiniteRowModelResult<User>>(this.baseUrl + 'api/Users', {
      params: {
        query: query
      }
    });
  }
}
