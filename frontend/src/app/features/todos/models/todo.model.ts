export enum TodoPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Urgent = 4
}

export enum TodoCategory {
  Personal = 1,
  Work = 2,
  Shopping = 3,
  Health = 4,
  Education = 5,
  Finance = 6,
  Other = 7
}

export interface Todo {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  
  // Enhanced fields
  priority: TodoPriority;
  category: TodoCategory;
  dueDate?: string;
  reminderDateTime?: string;
  reminderSent: boolean;
  notes?: string;
  tags?: string;
  estimatedMinutes: number;
  
  // Computed properties
  isOverdue: boolean;
  isUrgent: boolean;
  hasReminder: boolean;
  priorityDisplay: string;
  categoryDisplay: string;
  
  createdAt: string;
  updatedAt?: string;
  completedAt?: string;
}

export interface CreateTodoRequest {
  title: string;
  description?: string;
  priority?: TodoPriority;
  category?: TodoCategory;
  dueDate?: string;
  reminderDateTime?: string;
  notes?: string;
  tags?: string;
  estimatedMinutes?: number;
}

export interface UpdateTodoRequest {
  title?: string;
  description?: string;
  isCompleted?: boolean;
  priority?: TodoPriority;
  category?: TodoCategory;
  dueDate?: string;
  reminderDateTime?: string;
  notes?: string;
  tags?: string;
  estimatedMinutes?: number;
}

export interface TodoStats {
  totalTodos: number;
  completedTodos: number;
  pendingTodos: number;
  overdueTodos: number;
  todaysDueTodos: number;
  thisWeeksDueTodos: number;
  completionRate: number;
  todosByCategory: Record<TodoCategory, number>;
  todosByPriority: Record<TodoPriority, number>;
}

export const PriorityLabels = {
  [TodoPriority.Low]: { label: 'Low', color: 'text-green-600', bgColor: 'bg-green-100', emoji: '🟢' },
  [TodoPriority.Medium]: { label: 'Medium', color: 'text-yellow-600', bgColor: 'bg-yellow-100', emoji: '🟡' },
  [TodoPriority.High]: { label: 'High', color: 'text-orange-600', bgColor: 'bg-orange-100', emoji: '🟠' },
  [TodoPriority.Urgent]: { label: 'Urgent', color: 'text-red-600', bgColor: 'bg-red-100', emoji: '🔴' }
};

export const CategoryLabels = {
  [TodoCategory.Personal]: { label: 'Personal', color: 'text-blue-600', bgColor: 'bg-blue-100', emoji: '👤' },
  [TodoCategory.Work]: { label: 'Work', color: 'text-purple-600', bgColor: 'bg-purple-100', emoji: '💼' },
  [TodoCategory.Shopping]: { label: 'Shopping', color: 'text-green-600', bgColor: 'bg-green-100', emoji: '🛒' },
  [TodoCategory.Health]: { label: 'Health', color: 'text-red-600', bgColor: 'bg-red-100', emoji: '🏥' },
  [TodoCategory.Education]: { label: 'Education', color: 'text-indigo-600', bgColor: 'bg-indigo-100', emoji: '📚' },
  [TodoCategory.Finance]: { label: 'Finance', color: 'text-yellow-600', bgColor: 'bg-yellow-100', emoji: '💰' },
  [TodoCategory.Other]: { label: 'Other', color: 'text-gray-600', bgColor: 'bg-gray-100', emoji: '📝' }
};