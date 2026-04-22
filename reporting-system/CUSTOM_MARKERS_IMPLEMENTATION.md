# Custom Map Markers with Icons Implementation

## Overview
Implemented a feature to display custom markers with backend-provided icons on the map. The implementation is optimized to fetch data once when the map loads.

## Changes Made

### 1. **AppealService.js** - New Service Methods
Created two methods to fetch appeals with their locations and custom icons:

#### `getAppealLocations()`
- Fetches all appeals with their locations and icons
- Endpoint: `GET /api/appeals/locations`
- Returns: Array of appeals with `appealId`, `latitude`, `longitude`, and `categoryIconUrl`

#### `getAppealLocationsByDistrict(districtId)`
- Fetches appeals for a specific district
- Endpoint: `GET /api/appeals/locations?district=<districtId>`
- Used when user has a district assigned (optimizes data fetching)

Both methods return a normalized response structure:
```javascript
{
  success: boolean,
  appeals: Array,
  errors: string[]
}
```

### 2. **CityMap.js** - Enhanced Component

#### New State Variables
- `appeals` - Array of fetched appeals with location data
- `appealsLoading` - Loading state for appeals
- `appealsError` - Error state for appeals
- `appealsFetchedRef` - Ref to track if appeals have been fetched (optimization)

#### New useEffect Hook
- Triggers when `mapLoaded` becomes true
- Fetches appeals only once (using `appealsFetchedRef`)
- Intelligently chooses endpoint:
  - If user has a district → uses `getAppealLocationsByDistrict()` for filtered data
  - Otherwise → uses `getAppealLocations()` for all appeals

#### Custom Markers
Added rendering for appeal markers with custom icons:
```jsx
{appeals.map((appeal) => (
  <Marker
    key={appeal.appealId}
    position={{
      lat: appeal.latitude,
      lng: appeal.longitude
    }}
    title={appeal.appealId}
    icon={{
      url: appeal.categoryIconUrl,
      scaledSize: new window.google.maps.Size(32, 32),
      origin: new window.google.maps.Point(0, 0),
      anchor: new window.google.maps.Point(16, 32)
    }}
  />
))}
```

#### Icon Configuration
- **Size**: 32x32 pixels (scalable if needed)
- **Origin**: (0, 0) - Top-left corner of the icon image
- **Anchor**: (16, 32) - Centers horizontally, positions at bottom of icon
- **URL**: Dynamic from backend (`categoryIconUrl`)

## API Response Format
The backend endpoint should return an array of objects:
```json
[
  {
    "appealId": "97bbeced-f28a-4157-bf84-39c947d9f878",
    "latitude": 49.842415,
    "longitude": 24.155514,
    "categoryIconUrl": "icons/pothole.png"
  }
]
```

## Optimization Details
- **Single Request**: Appeals are fetched only once when the map loads (not on every render)
- **District-Aware**: Automatically uses filtered endpoint if user has a district
- **Error Handling**: Graceful fallback with error logging if fetch fails
- **Performance**: Uses `key={appeal.appealId}` for efficient list rendering

## Integration Points

### Frontend
- Icons should be served from `/public` directory (e.g., `/public/icons/pothole.png`)
- Backend URL: `categoryIconUrl: "icons/pothole.png"`

### Backend
- Create endpoint: `GET /api/appeals/locations`
- Optional filter: `GET /api/appeals/locations?district=<districtId>`
- Return proper CORS headers if needed
- Ensure icon paths are accessible via public static files

## Testing
To test the implementation:
1. Ensure your backend `/api/appeals/locations` endpoint is accessible
2. Verify icon URLs are correct and files exist in public directory
3. Check browser console for logs:
   - "Appeals loaded:" - Shows fetched appeals data
   - "Failed to fetch appeals:" - Shows any errors
4. Map should display custom markers with icons at specified coordinates

## Future Enhancements
- Add click handlers to markers for more info
- Add marker clustering for many appeals
- Add filtering/search functionality
- Add marker animation on load
- Customize info windows on marker click

