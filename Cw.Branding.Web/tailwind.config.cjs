/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./Views/**/*.cshtml",
        "./Areas/**/Views/**/*.cshtml",
        "./Pages/**/*.cshtml",
        "./wwwroot/js/**/*.{js,ts}",
        "./wwwroot/**/*.html"
    ],
    theme: {
        // Container + breakpoints theo spec
        container: {
            center: true,
            padding: {
                DEFAULT: "1rem",   // 16px
                md: "1.5rem",      // 24px
                lg: "2rem"         // 32px
            },
            screens: {
                sm: "640px",
                md: "768px",   // Tablet
                lg: "1024px",
                xl: "1280px"   // Desktop chính
            }
        },

        extend: {
            colors: {
                brand: {
                    primary: "#004F9F", // Medical Blue
                    accent: "#FFD43B",
                    bg: "#F9FAFB"
                },
                blue: {
                    50: "#e6edf5",
                    100: "#c5d5ea",
                    200: "#a3bdde",
                    300: "#7fa4d1",
                    400: "#5a8bc3",
                    500: "#356fb5",
                    600: "#004F9F",
                    700: "#003f80",
                    800: "#002f60",
                    900: "#002143"
                },
                yellow: {
                    50: "#fef6dc",
                    100: "#fdeeb8",
                    200: "#fce38f",
                    300: "#fbd866",
                    400: "#FFD43B",
                    500: "#f0bf2a",
                    600: "#d9a51f",
                    700: "#b38719",
                    800: "#8c6913",
                    900: "#686243"
                },
                neutral: {
                    50: "#F9FAFB"
                }
            },

            fontFamily: {
                heading: ["Montserrat", "ui-sans-serif", "system-ui", "sans-serif"],
                body: ["Roboto", "ui-sans-serif", "system-ui", "sans-serif"],
                brand: ['"Monsieur La Doulaise"', "cursive"],
                sans: ["Roboto", "ui-sans-serif", "system-ui", "sans-serif"]
            },

            spacing: {
                unit: "0.5rem",      // 8px
                "2unit": "1rem",     // 16px
                "3unit": "1.5rem",   // 24px
                "4unit": "2rem",     // 32px
                "section-y": "4rem"  // 64px
            },

            borderRadius: {
                sm: "4px",
                md: "8px",
                lg: "16px"
            },

            boxShadow: {
                card: "0 2px 8px rgba(0,0,0,0.08)",
                "card-hover": "0 4px 12px rgba(0,0,0,0.10)"
            }
        },

        screens: {
            sm: "640px",
            md: "768px",
            lg: "1024px",
            xl: "1280px"
        }
    },
    plugins: []
};
