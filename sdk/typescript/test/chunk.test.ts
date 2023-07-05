import { chunk } from '../lib/helpers/chunk'

describe('chunk', () => {
  it('should return empty array if input array is empty', () => {
    expect(chunk([], 1)).toEqual([])
  })

  it('should chunk array into chunks of given size', () => {
    expect(chunk([1, 2, 3, 4, 5], 3)).toEqual([
      [1, 2, 3],
      [4, 5],
    ])
  })
})
