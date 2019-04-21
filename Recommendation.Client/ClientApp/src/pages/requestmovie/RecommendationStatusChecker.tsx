import * as React from "react";
import { CircularProgress, Typography } from "@material-ui/core";
import { Utils } from "../../common/Utils";
import { RecommendationStatusDisplay } from "./RecommendationStatusView";
import { RecommendationStatus } from "./RequestMovie";

export interface RecommendationStatusCheckerProps {
  queuedRecommendationId: number;
  onComplete: (status: RecommendationStatus) => void;
}

export function RecommendationStatusChecker(props: RecommendationStatusCheckerProps) {
  const [status, setStatus] = React.useState(RecommendationStatus.Queued);

  async function queryRecommendationStatus(queuedRecommendationId: number): Promise<RecommendationStatus> {
    const response = await Utils.fetchBackend(
      `/api/recommendations/status?queuedRecommendationId=${queuedRecommendationId}`);
    if (!response.ok)
      return RecommendationStatus.Error;
    return +(await response.text());
  }

  const updateStatus = async (cancelTimer: () => void) => {
    const receivedStatus = await queryRecommendationStatus(props.queuedRecommendationId);
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
  }, [props.queuedRecommendationId]);

  return (
    <div className="recommendationstatus-checker">
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
