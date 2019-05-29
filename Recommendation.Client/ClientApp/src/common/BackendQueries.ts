
import * as DB from "../model/DB";
import * as Model from "../model/Model";
import { Utils } from "./Utils";
import { Authentication } from "./Authentication";

const queryTags = async (): Promise<DB.Tag[]> => {
  const response = await Utils.fetchBackend("/api/data/tags");

  if (!response.ok)
    return [];

  return response.json();
};

const queryRecommendationId = async (queuedRecommendationId: number) => {
  const response = await Utils.fetchBackend(
    `/api/recommendations/id?queuedRecommendationId=${queuedRecommendationId}`);

  if (!response.ok)
    return 0;

  return +(await response.text());
};

const stopRecommendationGeneration = async (queuedRecommendationId: number) => {
  await Utils.fetchBackend(
    `/api/recommendations/stop?queuedRecommendationId=${queuedRecommendationId}`, {
      method: "POST",
    });
};

const queryRecommendedMovies = async (recommendationId?: number): Promise<DB.RecommendedMovie[] | undefined> => {
  const user = Authentication.getCachedUser();

  let response: Response;

  if (recommendationId)
    response = await Utils.fetchBackend(
      `/api/data/recommendedmovies/${recommendationId}?userId=${user.id}`);
  else
    response = await Utils.fetchBackend(
      `/api/data/recommendedmovies/latest?userId=${user.id}`);

  if (!response.ok)
    return undefined;

  return response.json();
};

const queryMovies = async (): Promise<DB.Movie[] | undefined> => {
  const response = await Utils.fetchBackend("/api/data/movies");

  if (!response.ok)
    return undefined;

  return response.json();
};

const startRecommendationGeneration = async (userId: string, tagIds: number[]) => {
  const response = await Utils.fetchBackend(
    `/api/recommendations/QueueRecommendation?userId=${userId}`, {
      method: "POST",
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(tagIds),
    });

  if (!response.ok)
    return 0;

  return +(await response.text());
};

async function requestDeletion(movies: Model.UserMovie[]) {
  const user = Authentication.getCachedUser();

  const result = await Utils.fetchBackend(`/api/data/user/movies?userId=${user.id}`, {
    method: "DELETE",
    body: JSON.stringify(movies),
    headers: {
      "Accept": "application/json",
      "Content-Type": "application/json",
    },
  });

  return result.ok;
}

async function requestAddition(movies: Model.UserMovie[]) {
  const user = Authentication.getCachedUser();

  const result = await Utils.fetchBackend(`/api/data/user/movies?userId=${user.id}`, {
    method: "POST",
    body: JSON.stringify(movies),
    headers: {
      "Accept": "application/json",
      "Content-Type": "application/json",
    },
  });

  return result.ok;
}

export const BackendQueries = {
  Tags: {
    queryTags,
  },
  Recommendations: {
    queryRecommendationId,
    startRecommendationGeneration,
    stopRecommendationGeneration,
  },
  Movies: {
    queryRecommendedMovies,
    queryMovies,
  },
  UserMovies: {
    requestAddition,
    requestDeletion,
  },
};