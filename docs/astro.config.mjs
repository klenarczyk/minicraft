// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";

// https://astro.build/config
export default defineConfig({
    base: "/minicraft/",
    site: "https://klenarczyk.github.io",

    integrations: [
        starlight({
            title: "Minicraft",
            social: [
                {
                    icon: "github",
                    label: "GitHub",
                    href: "https://github.com/klenarczyk/minicraft",
                },
            ],
            sidebar: [
                {
                    label: "Start Here",
                    items: [
                        { label: "Project Overview", link: "/intro/overview/" },
                    ],
                },
                {
                    label: "Architecture",
                    autogenerate: { directory: "architecture" },
                },
            ],
        }),
    ],
});
