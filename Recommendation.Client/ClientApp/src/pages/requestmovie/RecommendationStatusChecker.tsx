import * as React from "react";
import { CircularProgress, Typography } from "@material-ui/core";
import { Utils } from "../../common/Utils";
import { RecommendationStatusDisplay } from "./RecommendationStatusView";
import { RecommendationStatus } from "./RequestMovie";

export interface RecommendationStatusCheckerProps {
  recommendationId: number;
  onComplete: (status: RecommendationStatus) => void;
}

export function RecommendationStatusChecker(props: RecommendationStatusCheckerProps) {
  const [status, setStatus] = React.useState(RecommendationStatus.Queued);
  async function queryRecommendationStatus(recommendationId: number): Promise<RecommendationStatus> {
    const response = await Utils.fetchBackend(`/api/recommendations/status?queuedRecommendationId=${recommendationId}`);
    if (!response.ok)
      return RecommendationStatus.Error;
    return +response.text();
  }
  const updateStatus = async (cancelTimer: () => void) => {
    const receivedStatus = await queryRecommendationStatus(props.recommendationId);
    if (receivedStatus !== RecommendationStatus.InProgress
      && receivedStatus !== RecommendationStatus.Queued) {
      props.onComplete(receivedStatus);
      cancelTimer();
    }
    setStatus(receivedStatus);
  };
  React.useEffect(() => {
    let cancelationFunc: () => void | null = null;
    const timer = setInterval(() => updateStatus(cancelationFunc), 1000);
    cancelationFunc = () => clearInterval(timer);
    return cancelationFunc;
  }, []);
  return (
    <div>
      {status === RecommendationStatus.Queued || status === RecommendationStatus.InProgress
        ?
        <>
          <Typography>Waiting for recommendation to finish</Typography>
          <CircularProgress />
        </>
        : undefined}

      <Typography>Status: <RecommendationStatusDisplay status={status} /></Typography>
    </div>
  );
}
