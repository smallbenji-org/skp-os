import type { ProjectStage } from "@/types";

export const STAGE_LABELS: Record<ProjectStage, string> = {
  Created: "Oprettet",
  Approved: "Godkendt",
  Submitted: "Afleveret",
  Evaluated: "Evalueret",
};

export const STAGE_ORDER: ProjectStage[] = [
  "Created",
  "Approved",
  "Submitted",
  "Evaluated",
];

export function stageLabel(stage: ProjectStage | null | undefined): string {
  if (!stage) return "Oprettet";
  return STAGE_LABELS[stage] ?? stage;
}

export function stageClass(stage: ProjectStage | null | undefined): string {
  const value = stage ?? "Created";
  const classes: Record<string, string> = {
    Created: "stage-created",
    Approved: "stage-approved",
    Submitted: "stage-submitted",
    Evaluated: "stage-evaluated",
  };
  return classes[value] ?? classes.Created;
}