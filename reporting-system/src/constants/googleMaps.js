export const googleMapsLibraries = ['places'];

export const googleMapsLoaderOptions = {
    id: 'script-loader',
    googleMapsApiKey: process.env.REACT_APP_GOOGLE_MAPS_API_KEY,
    libraries: googleMapsLibraries,
    language: 'uk',
    region: 'UA'
};
