import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoService } from '../../services/todo.service';
import { Todo, CreateTodoRequest } from '../../models/todo.model';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './todo-list.component.html',
  styleUrls: ['./todo-list.component.css']
})
export class TodoListComponent implements OnInit {
  todos: Todo[] = [];
  newTodo: CreateTodoRequest = { title: '', description: '' };
  loading = false;
  error = '';

  constructor(
    private todoService: TodoService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadTodos();
  }

  loadTodos(): void {
    this.loading = true;
    this.error = '';
    
    this.todoService.getTodos().subscribe({
      next: (todos) => {
        this.todos = todos;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to load todos';
        this.loading = false;
        console.error('Error loading todos:', error);
      }
    });
  }

  addTodo(): void {
    if (!this.newTodo.title.trim()) return;

    this.loading = true;
    this.todoService.createTodo(this.newTodo).subscribe({
      next: (todo) => {
        this.todos.unshift(todo);
        this.newTodo = { title: '', description: '' };
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to create todo';
        this.loading = false;
        console.error('Error creating todo:', error);
      }
    });
  }

  toggleTodo(todo: Todo): void {
    this.todoService.toggleTodo(todo.id, !todo.isCompleted).subscribe({
      next: (updatedTodo) => {
        const index = this.todos.findIndex(t => t.id === todo.id);
        if (index !== -1) {
          this.todos[index] = updatedTodo;
        }
      },
      error: (error) => {
        this.error = 'Failed to update todo';
        console.error('Error updating todo:', error);
      }
    });
  }

  deleteTodo(id: number): void {
    if (!confirm('Are you sure you want to delete this todo?')) return;

    this.todoService.deleteTodo(id).subscribe({
      next: () => {
        this.todos = this.todos.filter(todo => todo.id !== id);
      },
      error: (error) => {
        this.error = 'Failed to delete todo';
        console.error('Error deleting todo:', error);
      }
    });
  }

  logout(): void {
    this.authService.logout();
  }

  get user() {
    return this.authService.getUserData();
  }

  trackById(index: number, todo: Todo): number {
    return todo.id;
  }
}