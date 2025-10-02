import { MenuItem } from '../models/menu.model';

export class Menu {
  public static pages: MenuItem[] = [
    {
      group: 'Task Management',
      separator: false,
      items: [
        {
          icon: 'assets/icons/heroicons/outline/menu.svg',
          label: 'Dashboard',
          route: '/dashboard/home',
        },
        {
          icon: 'assets/icons/heroicons/outline/clipboard-list.svg',
          label: 'My Todos',
          route: '/dashboard/todos',
        },
      ],
    },
    {
      group: 'Settings',
      separator: true,
      items: [
        {
          icon: 'assets/icons/heroicons/outline/cog.svg',
          label: 'Settings',
          route: '/settings',
        },
        {
          icon: 'assets/icons/heroicons/outline/bell.svg',
          label: 'Notifications',
          route: '/notifications',
        },
      ],
    },
  ];
}
