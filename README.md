# Paint

## Thông tin sinh viên

- 20120206 - Nguyễn Ngọc Thùy
- 20120581 - Nguyễn Thị Ngọc Thành
- 20120589 - Nguyễn Hạnh Thư

## Thông tin project

### Cách chạy project

Mở file solution **Paint.sln** và nhấn F5 để chạy project

### Các chức năng/yêu cầu đã hoàn thành

#### Technical details

- [x] Design patterns: Singleton, Factory, Prototype
- [x] Plugin architecture

#### Core requirements (5 points)

- [x] 1. Dynamically load all graphic objects that can be drawn from external DLL files
- [x] 2. The user can choose which object to draw
- [x] 3. The user can see the preview of the object they want to draw
- [x] 4. The user can finish the drawing preview and their change becomes permanent with previously drawn objects
- [x] 5. The list of drawn objects can be saved and loaded again for continuing later
- [x] 6. Save and load all drawn objects as an image in bmp/png/jpg format (rasterization)

#### Basic graphic objects

- [x] 1. Line: controlled by two points, the starting point, and the endpoint
- [x] 2. Rectangle: controlled by two points, the left top point, and the right bottom point
- [x] 3. Ellipse: controlled by two points, the left top point, and the right bottom point
- [x] 4. Circle: controlled by two points, the left top point, and the right bottom point
- [x] 5. Square: controlled by two points, the left top point, and the right bottom point

#### Improvements

- [x] 1. Allow user to change the color, pen width, stroke type
- [x] 2. Allow user to fill object, change object's fill color
- [x] 3. Adding text to the list of drawable objects
- [x] 4. Adding image to the canvas
- [x] 5. Select a single element for editing again: Transforming horizontally and vertically, rotate the image, drag & drop
- [x] 6. Zooming
- [x] 7. Cut / Copy / Paste
- [x] 8. Undo, Redo
- [x] 9. Add hooking to support global shortcut key for save objects/image, import objects/image, create new file, undo & redo

### Những phần để xem xét cộng điểm

- Thiết kế giao diện dễ nhìn và thân thiện với người dùng

### Điểm đề nghị: 10

### Link demo: https://youtu.be/G3rFeXqLlCs
