import { LedgerState } from '../generated'

/**
 * Exhausts a paginated API resource and returns all the results.
 */
export const exhaustPaginationWithLedgerState = async <T>(
  queryFunction: (
    cursor?: string
  ) => Promise<{
    ledger_state: LedgerState
    items: T[]
    next_cursor?: string | null
  }>,
  start?: string
) => {
  let next_cursor: string | null | undefined = start
  const aggregatedEntities: T[] = []

  let ledgerState: LedgerState

  do {
    const queryFunctionResponse: {
      next_cursor?: string | null
      items: T[]
      ledger_state: LedgerState
    } = await queryFunction(next_cursor)
    ledgerState = queryFunctionResponse.ledger_state
    aggregatedEntities.push(...queryFunctionResponse.items)
    next_cursor = queryFunctionResponse.next_cursor
  } while (next_cursor)

  return {
    aggregatedEntities,
    ledger_state: ledgerState,
  }
}
