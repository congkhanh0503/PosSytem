/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        barber: {
          dark: '#0e0e11',
          card: '#16161c',
          border: '#262631',
          gold: '#e5a93c',
          goldHover: '#f5b84d',
          goldMuted: '#946e27',
          accent: '#c59b27',
          gray: '#8f909d',
          light: '#f4f4f6'
        }
      }
    },
  },
  plugins: [],
}
