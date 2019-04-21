import * as React from "react";
import { RecommendationStatus } from "./RequestMovie";

export interface RecommendationStatusViewProps {
  status: RecommendationStatus;
}

export function RecommendationStatusDisplay(props: RecommendationStatusViewProps) {
  switch (props.status) {
    case RecommendationStatus.DoesNotExist:
      return <>Failed to start</>;
    case RecommendationStatus.Error:
      return <>Error</>;
    case RecommendationStatus.Finished:
      return <>Finished</>;
    case RecommendationStatus.InProgress:
      return <>In progress</>;
    case RecommendationStatus.Queued:
      return <>Queued</>;
    case RecommendationStatus.Stopped:
      return <>Stopped</>;
  }
  return <></>;
}
