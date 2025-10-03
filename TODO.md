# Tasky App - TODOs

## UI/Design Issues

- [x] **Notification Page Alignment** ✅
    - ~~SVG icons next to "Created", "Completed", and "Deleted" status labels are not properly aligned with the text~~
    - ~~Need to adjust vertical alignment to match text height~~
    - Fixed: Added flex-shrink-0 classes for proper icon alignment

- [x] **Notification Bell Issues** ✅
    - ~~Top-right notification bell SVG icon is not visible~~
    - ~~Need to investigate why the icon isn't rendering properly~~
    - Fixed: Updated icon size from h-5 w-5 to h-6 w-6 and added flex-shrink-0

- [x] **Notification Item Alignment** ✅
    - ~~SVG icons in "New Todo Added" notifications are not properly centered~~
    - ~~Fix alignment of notification content elements~~
    - Fixed: Added flex-shrink-0 classes and improved line-height for text content

## Notification System

- [x] **Notification Counter** ✅
    - ~~Counter shows "12 unread" but only 1 notification is visible~~
    - ~~Issue: Counter includes old notifications that weren't created in current session~~
    - ~~Implement proper notification count reset or filtering~~
    - Fixed: Now calculating unread count from loaded notifications for consistency

## Functionality Issues

- [x] **Reminder Emails Working** ✅
    - ~~Investigate why reminder emails are not being sent~~
    - Successfully implemented and tested email reminder system
    - TodoService → NotificationService via HTTP API and RabbitMQ
    - Email processing confirmed working via logs

## Additional Tasks

- [ ] Review notification lifecycle management
- [ ] Implement notification clearing functionality
- [ ] Test notification system with different user scenarios