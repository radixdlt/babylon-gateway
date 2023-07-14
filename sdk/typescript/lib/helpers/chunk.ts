/**
 * Creates an array of elements split into groups the length of `chunkSize`.
 * If `array` can't be split evenly, the final chunk will be the remaining
 * elements.
 * 
 * @param array input array to be parsed
 * @param {number} chunkSize lenght of each chunk
 * @returns {Array} array of chunks
 * 
 * @example
 *
 * chunk(['a', 'b', 'c', 'd'], 3)
 * // => [['a', 'b', 'c'], ['d']]
 */
export const chunk = <T>(array: T[], chunkSize: number): T[][] => {
  const chunks = []
  for (let i = 0, length = array.length; i < length; i += chunkSize)
    chunks.push(array.slice(i, i + chunkSize))
  return chunks
}
