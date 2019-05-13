
import * as DB from "../model/DB";
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
};