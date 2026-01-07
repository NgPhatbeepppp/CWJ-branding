# News Management System

This document provides an overview of the news management functionality implemented in the CW Branding application.

## Features

### Admin Panel
- **Full CRUD Operations**: Create, Read, Update, and Delete news articles
- **Bilingual Support**: Manage content in both English and Vietnamese
- **Image Upload**: Upload thumbnail images with automatic validation
- **Publishing Control**: Control publication status and dates
- **SEO Optimization**: Meta title and description fields for both languages

### Security
- **Authentication Required**: Admin panel protected by cookie authentication
- **CSRF Protection**: ValidateAntiForgeryToken on all POST operations
- **File Validation**: 
  - Only image files allowed (.jpg, .jpeg, .png, .gif, .webp)
  - Maximum file size: 5MB
  - Unique file naming prevents overwrites

## Architecture

### Service Layer
- `INewsService`: Interface defining news operations
- `NewsService`: Implementation with Entity Framework Core

### Controllers
- `Areas/Admin/Controllers/NewsController`: Admin CRUD operations
- `Controllers/NewsController`: Public news display

### Views
- `Areas/Admin/Views/News/Index.cshtml`: List all news
- `Areas/Admin/Views/News/Create.cshtml`: Create new article
- `Areas/Admin/Views/News/Edit.cshtml`: Edit existing article
- `Areas/Admin/Views/News/Delete.cshtml`: Delete confirmation

## Usage

### Admin Access
Navigate to `/en/admin/news` (requires authentication)

### Available Operations

#### Create News Article
1. Click "Create New Article" button
2. Fill in English and Vietnamese content
3. Optionally upload a thumbnail image
4. Set publishing status and date
5. Click "Create Article"

#### Edit News Article
1. Click "Edit" button on any article
2. Modify content as needed
3. Upload new thumbnail (optional)
4. Click "Update Article"

#### Delete News Article
1. Click "Delete" button on any article
2. Review article details
3. Confirm deletion

### Client Display
- List view: `/en/news`
- Detail view: `/en/news/{slug}`

## Database Schema

The `News` table includes:
- Basic fields: Id, TitleEn, TitleVi, SummaryEn, SummaryVi, ContentEn, ContentVi
- SEO fields: MetaTitleEn, MetaTitleVi, MetaDescEn, MetaDescVi
- Slug fields: SlugEn, SlugVi
- Publishing: IsPublished, PublishedAt
- Media: ThumbnailPath
- Audit: CreatedAt, UpdatedAt

## File Storage

Uploaded thumbnails are stored in:
```
wwwroot/uploads/news/
```

File naming convention:
```
{Guid}.{extension}
```

## API Reference

### INewsService Methods

```csharp
Task<IEnumerable<News>> GetAllNewsAsync()
Task<News?> GetNewsByIdAsync(int id)
Task<News?> GetNewsBySlugAsync(string slug, string lang)
Task<News> CreateNewsAsync(News news)
Task<News> UpdateNewsAsync(News news)
Task<bool> DeleteNewsAsync(int id)
Task<IEnumerable<News>> GetPublishedNewsAsync(int count = 10)
```

## Configuration

The NewsService is registered in `Program.cs`:
```csharp
builder.Services.AddScoped<INewsService, NewsService>();
```

## Future Enhancements

Potential improvements:
- Rich text editor integration (TinyMCE, CKEditor)
- Image resizing and optimization
- Categories/tags for news articles
- Search and filtering
- Draft preview functionality
- Scheduled publishing
- Multi-author support
- Analytics integration
