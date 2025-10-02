import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Todo, CreateTodoRequest, UpdateTodoRequest } from '../models/todo.model';

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  private readonly API_URL = environment.apiUrl.todo;

  constructor(private http: HttpClient) {}

  getTodos(): Observable<Todo[]> {
    return this.http.get<{todos: Todo[], totalCount: number, page: number, pageSize: number, totalPages: number}>(this.API_URL)
      .pipe(map(response => response.todos || []));
  }

  createTodo(todo: CreateTodoRequest): Observable<Todo> {
    return this.http.post<Todo>(this.API_URL, todo);
  }

  updateTodo(id: number, todo: UpdateTodoRequest): Observable<Todo> {
    return this.http.put<Todo>(`${this.API_URL}/${id}`, todo);
  }

  deleteTodo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }

  toggleTodo(id: number, isCompleted: boolean): Observable<Todo> {
    return this.updateTodo(id, { isCompleted });
  }
}