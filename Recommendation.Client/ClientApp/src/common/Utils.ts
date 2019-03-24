// tslint:disable-next-line:no-implicit-dependencies
import { History } from "history";

const backendUrl = "http://localhost:4000";

function fetchBackend(path: string, options?: RequestInit) {
    const url = backendUrl + path;
    const optionsWithCookies: RequestInit = {
        credentials: "include",
        ...options,
    };
    return fetch(url, optionsWithCookies);
}

function createOnNavigationClick(history: History, path: string) {
    return () => {
        history.push(path);
    };
}

const DEFAULT_MOVIE_IMAGE_URL = "https://i.imgur.com/Z2MYNbj.png/large_movie_poster.png";

export const Utils = {
    fetchBackend,
    createOnNavigationClick,
    DEFAULT_MOVIE_IMAGE_URL,
};