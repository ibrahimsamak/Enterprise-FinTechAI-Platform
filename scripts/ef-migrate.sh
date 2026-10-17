#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

MODE="${1:-all}"
NAME_SUFFIX="${2:-Initial}"

CONTEXTS=(
  "CustomerDbContext|src/modules/FinAI.Modules.Customer|src/bootstrap/FinAI.Api"
  "PaymentDbContext|src/modules/FinAI.Modules.Payment|src/bootstrap/FinAI.Api"
  "AccountDbContext|src/modules/FinAI.Modules.Account|src/bootstrap/FinAI.Workers"
  "FraudDbContext|src/modules/FinAI.Modules.Fraud|src/bootstrap/FinAI.Workers"
  "RagDbContext|src/ai/FinAI.AI.Rag|src/bootstrap/FinAI.Workers"
)

add_migration() {
  local ctx="$1" proj="$2" startup="$3"
  local short="${ctx%DbContext}"
  echo ">> migrations add ${NAME_SUFFIX}${short} ($ctx)"
  dotnet ef migrations add "${NAME_SUFFIX}${short}" -p "$proj" -s "$startup" -c "$ctx"
}

update_database() {
  local ctx="$1" proj="$2" startup="$3"
  echo ">> database update ($ctx)"
  dotnet ef database update -p "$proj" -s "$startup" -c "$ctx"
}

for row in "${CONTEXTS[@]}"; do
  IFS='|' read -r ctx proj startup <<< "$row"
  case "$MODE" in
    add)    add_migration "$ctx" "$proj" "$startup" ;;
    update) update_database "$ctx" "$proj" "$startup" ;;
    all)    add_migration "$ctx" "$proj" "$startup"
            update_database "$ctx" "$proj" "$startup" ;;
    *) echo "Unknown mode: $MODE (expected: add | update | all)" >&2; exit 1 ;;
  esac
done

echo "Done ($MODE)."
