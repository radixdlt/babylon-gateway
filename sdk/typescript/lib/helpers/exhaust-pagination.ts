/**
 * Exhausts a paginated API resource and returns all the results.
 */
export const exhaustPagination = async <T>(
  queryFunction: (
    cursor?: string
  ) => Promise<{ items: T[]; next_cursor?: string | null }>,
  start?: string
): Promise<T[]> => {
  let next_cursor: string | null | undefined = start
  const aggregatedEntities: T[] = []

  do {
    const queryFunctionResponse: {
      next_cursor?: string | null
      items: T[]
    } = await queryFunction(next_cursor)
    aggregatedEntities.push(...queryFunctionResponse.items)
    next_cursor = queryFunctionResponse.next_cursor
  } while (next_cursor)

  return aggregatedEntities
}
