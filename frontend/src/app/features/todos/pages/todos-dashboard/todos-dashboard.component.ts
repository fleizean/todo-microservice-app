import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoService } from '../../services/todo.service';
import { Todo, CreateTodoRequest, TodoPriority, TodoCategory, PriorityLabels, CategoryLabels } from '../../models/todo.model';
import { AuthService } from '../../../../core/services/auth.service';
import { AngularSvgIconModule } from 'angular-svg-icon';

@Component({
  selector: 'app-todos-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, AngularSvgIconModule],
  templateUrl: './todos-dashboard.component.html',
  styleUrls: ['./todos-dashboard.component.css']
})
export class TodosDashboardComponent implements OnInit {
  todos: Todo[] = [];
  newTodo: CreateTodoRequest = { 
    title: '', 
    description: '',
    priority: TodoPriority.Medium,
    category: TodoCategory.Personal,
    estimatedMinutes: 0
  };
  loading = false;
  error = '';
  showAddForm = false;
  filterStatus: 'all' | 'pending' | 'completed' = 'all';
  
  // Enhanced properties
  readonly TodoPriority = TodoPriority;
  readonly TodoCategory = TodoCategory;
  readonly PriorityLabels = PriorityLabels;
  readonly CategoryLabels = CategoryLabels;
  
  // Filter and sort options
  selectedPriority: TodoPriority | null = null;
  selectedCategory: TodoCategory | null = null;
  sortBy: 'createdAt' | 'dueDate' | 'priority' | 'title' = 'createdAt';
  sortDesc = true;

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
        this.resetNewTodo();
        this.showAddForm = false;
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Failed to create todo';
        this.loading = false;
        console.error('Error creating todo:', error);
      }
    });
  }
  
  resetNewTodo(): void {
    this.newTodo = { 
      title: '', 
      description: '',
      priority: TodoPriority.Medium,
      category: TodoCategory.Personal,
      estimatedMinutes: 0
    };
  }
  
  clearFilters(): void {
    this.selectedPriority = null;
    this.selectedCategory = null;
    this.filterStatus = 'all';
  }
  
  formatDueDate(dueDate: string | undefined): string {
    if (!dueDate) return '';
    const date = new Date(dueDate);
    const now = new Date();
    const diffTime = date.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Tomorrow';
    if (diffDays === -1) return 'Yesterday';
    if (diffDays < 0) return `${Math.abs(diffDays)} days ago`;
    return `${diffDays} days`;
  }
  
  formatEstimatedTime(minutes: number): string {
    if (minutes === 0) return '';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    
    if (hours > 0 && mins > 0) return `${hours}h ${mins}m`;
    if (hours > 0) return `${hours}h`;
    return `${mins}m`;
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

  get user() {
    return this.authService.getUserData();
  }

  get completedTodos() {
    return this.todos.filter(todo => todo.isCompleted);
  }

  get pendingTodos() {
    return this.todos.filter(todo => !todo.isCompleted);
  }

  get completionPercentage() {
    if (this.todos.length === 0) return 0;
    return Math.round((this.completedTodos.length / this.todos.length) * 100);
  }

  get filteredTodos() {
    let filtered = this.todos;
    
    // Filter by completion status
    switch(this.filterStatus) {
      case 'pending': 
        filtered = filtered.filter(todo => !todo.isCompleted);
        break;
      case 'completed': 
        filtered = filtered.filter(todo => todo.isCompleted);
        break;
    }
    
    // Filter by priority
    if (this.selectedPriority !== null) {
      filtered = filtered.filter(todo => todo.priority === this.selectedPriority);
    }
    
    // Filter by category
    if (this.selectedCategory !== null) {
      filtered = filtered.filter(todo => todo.category === this.selectedCategory);
    }
    
    // Sort todos
    filtered.sort((a, b) => {
      let comparison = 0;
      
      switch(this.sortBy) {
        case 'title':
          comparison = a.title.localeCompare(b.title);
          break;
        case 'priority':
          comparison = b.priority - a.priority; // Higher priority first
          break;
        case 'dueDate':
          const aDate = a.dueDate ? new Date(a.dueDate).getTime() : Number.MAX_VALUE;
          const bDate = b.dueDate ? new Date(b.dueDate).getTime() : Number.MAX_VALUE;
          comparison = aDate - bDate;
          break;
        default: // createdAt
          comparison = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
      }
      
      return this.sortDesc ? -comparison : comparison;
    });
    
    return filtered;
  }
  
  get overdueTodos() {
    return this.todos.filter(todo => todo.isOverdue);
  }
  
  get urgentTodos() {
    return this.todos.filter(todo => todo.isUrgent && !todo.isCompleted);
  }
  
  get todaysDueTodos() {
    const today = new Date().toDateString();
    return this.todos.filter(todo => 
      todo.dueDate && new Date(todo.dueDate).toDateString() === today && !todo.isCompleted
    );
  }

  trackById(index: number, todo: Todo): number {
    return todo.id;
  }
}