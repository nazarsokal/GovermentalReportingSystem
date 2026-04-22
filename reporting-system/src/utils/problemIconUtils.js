/**
 * Problem Icon Utilities
 * Helper functions for working with PNG problem icons
 */

import { PROBLEM_ICON_NAMES, getIconPath, getIconSrcSet } from '../images/icons/problems';

/**
 * PNG Icon sizes (in pixels)
 */
export const ICON_SIZES = {
  SMALL: 32,
  MEDIUM: 64,
  LARGE: 128,
  XLARGE: 256,
};

/**
 * Problem type to display label mapping
 */
export const PROBLEM_TYPE_LABELS = {
  POTHOLE: 'Pothole',
  TRAFFIC_SIGNAL: 'Traffic Signal',
  STREETLIGHT: 'Street Light',
  WATER_LEAK: 'Water Leak',
  DAMAGED_ROAD: 'Damaged Road',
  OTHER: 'Other',
};

/**
 * Problem type to description mapping
 */
export const PROBLEM_TYPE_DESCRIPTIONS = {
  POTHOLE: 'Road surface damage or holes',
  TRAFFIC_SIGNAL: 'Broken or malfunctioning traffic lights',
  STREETLIGHT: 'Non-functional street light',
  WATER_LEAK: 'Water pipe or main leak',
  DAMAGED_ROAD: 'General road surface damage',
  OTHER: 'Other infrastructure problem',
};

/**
 * Get the appropriate size pixel value for a size name
 * @param {string} sizeName - Size name ('SMALL', 'MEDIUM', 'LARGE', 'XLARGE')
 * @returns {number} Pixel size
 */
export function getSizePixels(sizeName) {
  return ICON_SIZES[sizeName] || ICON_SIZES.MEDIUM;
}

/**
 * Render an img tag for a problem icon
 * @param {string} type - Problem type (e.g., 'POTHOLE')
 * @param {string} size - Size name or number (defaults to 'MEDIUM' or 64px)
 * @param {object} options - Additional options
 * @returns {object} Image element props
 */
export function getProblemIconProps(type, size = 'MEDIUM', options = {}) {
  const sizePixels = typeof size === 'number' ? size : ICON_SIZES[size];

  return {
    src: getIconPath(type, sizePixels),
    alt: options.alt || PROBLEM_TYPE_LABELS[type] || 'Problem icon',
    width: sizePixels,
    height: sizePixels,
    title: options.title || PROBLEM_TYPE_DESCRIPTIONS[type],
    loading: 'lazy',
    ...options,
  };
}

/**
 * Get responsive image props with srcset
 * Useful for responsive designs
 */
export function getResponsiveIconProps(type, options = {}) {
  return {
    src: getIconPath(type, ICON_SIZES.MEDIUM),
    srcSet: getIconSrcSet(type),
    sizes: options.sizes || '(max-width: 600px) 32px, 64px',
    alt: options.alt || PROBLEM_TYPE_LABELS[type] || 'Problem icon',
    title: options.title || PROBLEM_TYPE_DESCRIPTIONS[type],
    loading: 'lazy',
    ...options,
  };
}

/**
 * Get label for a problem type
 * @param {string} type - Problem type
 * @returns {string} Display label
 */
export function getProblemTypeLabel(type) {
  return PROBLEM_TYPE_LABELS[type] || type;
}

/**
 * Get description for a problem type
 * @param {string} type - Problem type
 * @returns {string} Description
 */
export function getProblemTypeDescription(type) {
  return PROBLEM_TYPE_DESCRIPTIONS[type] || '';
}

/**
 * List all available problem types
 * @returns {array} Array of problem types
 */
export function getAllProblemTypes() {
  return Object.keys(PROBLEM_ICON_NAMES);
}

/**
 * Create HTML img element attributes for a problem icon
 * @param {string} type - Problem type
 * @param {string} csvSize - Size name
 * @param {object} htmlAttrs - Additional HTML attributes
 * @returns {string} HTML attributes string
 */
export function createIconHtmlAttrs(type, size = 'MEDIUM', htmlAttrs = {}) {
  const props = getProblemIconProps(type, size, htmlAttrs);

  return Object.entries(props)
    .map(([key, value]) => `${key}="${value}"`)
    .join(' ');
}

export default {
  ICON_SIZES,
  PROBLEM_TYPE_LABELS,
  PROBLEM_TYPE_DESCRIPTIONS,
  getSizePixels,
  getProblemIconProps,
  getResponsiveIconProps,
  getProblemTypeLabel,
  getProblemTypeDescription,
  getAllProblemTypes,
  createIconHtmlAttrs,
};

